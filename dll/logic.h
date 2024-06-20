#pragma once
#include "dll.h"

struct Scene;
struct Robot;

struct Item {
    Scene* scene{};
    std::string name;
    xx::XY pos{};
    float radius{}, healthPoint{};

    bool Hit(Robot& attacker);                          // return true: death
};

struct Tree : Item {};

using Trees = xx::BlockLink<Tree>;
using Tree_w = Trees::WeakType;

struct Robot : Item {
    float searchDelaySeconds{};
    float moveStepSeconds, moveSpeed{};
    float attackDelaySeconds{}, attackRange{}, damage{};
    Tree_w target;

    // let target = nearest tree.
    // return true: success    false: not found
    xx::Task<bool> SearchTarget();

    // move a while.
    // return 0: moving   1: reached   2: lost target
    xx::Task<int> MoveToTarget();

    // target.healthPoint -= damage, then sleep attackDelaySeconds.
    // return 0: attack success   1: attack success + killed target   2: lost target   3: can't reach
    xx::Task<int> Attack();

    xx::Task<> Update = Update_();
    xx::Task<> Update_();
};

using Robots = xx::BlockLink<Robot>;
using Robot_w = Robots::WeakType;

struct Scene {
    /*

  0       15  25           50    62    75          100
  ┌────────────────────────────────────────────────┐
  │                                                │
  │                             ┌──┐               │
15│     robot1                  │1 │               │
  │     ───►                    └──┘               │
  │                                                │
  │                                   trees        │
  │                                   ┌──┐         │
  │                                   │2 │         │
35│                                   └──┘         │
  │                                                │
  │                                                │
  │                                                │
  │                           ┌──┐                 │
50│      robot2               │3 │                 │
  │      ───►                 └──┘                 │
  │                                                │
  │                                                │
  └────────────────────────────────────────────────┘
 70

    */
    static constexpr float cFramePerSeconds{ 10.f };
    static constexpr float cFrameDelaySeconds{ 1.f / cFramePerSeconds };

    bool gameOver{};
    float timePool{}, now{};

    xx::BlockLink<Tree> trees;
    xx::BlockLink<Robot> robots;

    xx::Task<> Logic = Logic_();
    xx::Task<> Logic_() {
        Dump("play");
        // make some trees
        {
            auto&& o = trees.Emplace();
            o.scene = this;
            o.name = "tree1";
            o.pos = { 62, 15 };
            o.radius = 10;
            o.healthPoint = 3;
        }
        {
            auto&& o = trees.Emplace();
            o.scene = this;
            o.name = "tree2";
            o.pos = { 75, 33 };
            o.radius = 10;
            o.healthPoint = 4;
        }
        {
            auto&& o = trees.Emplace();
            o.scene = this;
            o.name = "tree3";
            o.pos = { 60, 50 };
            o.radius = 10;
            o.healthPoint = 5;
        }
        // make 2 robots
        {
            auto&& o = robots.Emplace();
            o.scene = this;
            o.name = "robot1";
            o.pos = { 15, 15 };
            o.radius = 10;
            o.healthPoint = 100;
            o.searchDelaySeconds = 0.5f;
            o.moveStepSeconds = 0.5f;
            o.moveSpeed = 100.f / 5.f / cFramePerSeconds;
            o.attackDelaySeconds = 1.f;
            o.attackRange = 15;
            o.damage = 1;
        }
        {
            auto&& o = robots.Emplace();
            o.scene = this;
            o.name = "robot2";
            o.pos = { 15, 50 };
            o.radius = 10;
            o.healthPoint = 100;
            o.searchDelaySeconds = 0.5f;
            o.moveStepSeconds = 0.5f;
            o.moveSpeed = 100.f / 5.f / cFramePerSeconds;
            o.attackDelaySeconds = 1.f;
            o.attackRange = 15;
            o.damage = 1;
        }
        co_yield 0;

        while (!trees.Empty() && !robots.Empty()) {
            robots.ForeachFlags([](Robot& robot)->xx::ForeachResult {
                if (robot.Update()) return xx::ForeachResult::RemoveAndContinue;
                else return  xx::ForeachResult::Continue;
                });
            co_yield 0;
        }

        Dump("game over");
        gameOver = true;
    }

    void Update(float delta) {
        now += delta;
        timePool += delta;
        if (timePool >= cFrameDelaySeconds) {
            timePool -= cFrameDelaySeconds;
            Logic();
        }
    }

    template<typename...Args>
    void Dump(Args&&... args) {
#if !ENABLE_SCENE_PERFORMANCE_TEST
        auto s = std::to_string(now);
        if (s.size() < 10) s.append(10 - s.size(), ' ');
        xx::Cout("[", s, "] ");
        xx::CoutN(std::forward<Args>(args)...);
#endif
    }
};


inline xx::Task<> Robot::Update_() {
LabSearch:
    if (!co_await SearchTarget()) goto LabSearch;       // keep searching

LabMove:
    switch (co_await MoveToTarget()) {
    case 0: goto LabMove;                               // far away from the target: keep moving
    case 1: goto LabAttack;                             // reached target: begin attack
    case 2: goto LabSearch;                             // lost target: search again
    default: XX_ASSUME(false);
    }

LabAttack:
    switch (co_await Attack()) {
    case 0: goto LabAttack;                             // attack success: keep attacking
    case 1: goto LabLoot;                               // attack success + target is dead: loot
    case 2: goto LabSearch;                             // lost target: search again
    case 3: goto LabMove;                               // far away from the target: move to target
    default: XX_ASSUME(false);
    }

LabLoot:
    // todo
    goto LabSearch;
}

inline bool Item::Hit(Robot& attacker) {
    healthPoint -= attacker.damage;
    scene->Dump(attacker.name, " hit ", name, " and deal ", attacker.damage, " damage ! tree's HP = ", healthPoint);
    return healthPoint <= 0;
}

inline xx::Task<bool> Robot::SearchTarget() {
    scene->Dump(name, " call SearchTarget() ******************************");

    target.Reset();
    float minDistancePow2{ std::numeric_limits<float>::max() };
    scene->trees.ForeachFlags([&](Tree& tree)->void {
        if (auto mag2 = (tree.pos - pos).Mag2(); mag2 < minDistancePow2) {
            minDistancePow2 = mag2;
            target = tree;
        }
        });

    for (auto e = scene->now + searchDelaySeconds; scene->now < e;) {
        scene->Dump(name, " is waiting search delay. left seconds = ", e - scene->now);
        co_yield 0;
    }

    if (target) {
        scene->Dump(name, " found target: ", target().name, " !");
        co_return true;
    }
    co_return false;
}

inline xx::Task<int> Robot::MoveToTarget() {
    scene->Dump(name, " call MoveToTarget() ******************************");
    for (auto e = scene->now + moveStepSeconds; scene->now < e;) {
        if (!target) {
            scene->Dump(name, " MoveToTarget(): lost target");
            co_return 2;
        }
        auto&& tree = target();
        auto d = tree.pos - pos;
        auto r = radius + attackRange;
        auto rr = (tree.radius + r) * (tree.radius + r);
        auto mag2 = d.Mag2();
        if (rr > mag2) {
            scene->Dump(name, " MoveToTarget(): reached");
            co_return 1;
        }
        if (mag2 > 0.f) {
            auto v = d / std::sqrt(mag2);		// normalize
            pos += v * moveSpeed;                       // move
            scene->Dump(name, " is moving to ", target().name, ". pos = ", pos.x, ", ", pos.y);
        }
        co_yield 0;
    }
    co_return 0;                                        // moving
}

inline xx::Task<int> Robot::Attack() {
    scene->Dump(name, " call Attack() ******************************");
    if (!target) {
        scene->Dump(name, " Attack(): lost target");
        co_return 2;
    }

    auto&& tree = target(); // unsafe: avoid visit after yield

    auto r = radius + attackRange;
    auto rr = (tree.radius + r) * (tree.radius + r);
    if (auto mag2 = (tree.pos - pos).Mag2(); mag2 > rr) {
        scene->Dump(name, " Attack(): out of attack range");
        co_return 3;
    }

    bool targetIsDead = tree.Hit(*this);
    if (targetIsDead) {
        scene->Dump(name, " killed ", tree.name, " !!!!!!!!!!!!!!!!!!!!!");
        scene->trees.Remove(target);    // can't visit "tree" after this line
    }

    // simulate cast delay
    for (auto e = scene->now + attackDelaySeconds; scene->now < e;) {
        scene->Dump(name, " is waiting for attack cast delay. left seconds = ", e - scene->now);
        co_yield 0;
    }

    if (targetIsDead) co_return 1;                      // attack success + killed target
    co_return 0;                                        // attack success
}
