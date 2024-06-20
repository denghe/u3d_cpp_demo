#pragma once
#include "dll.h"

namespace Logic1 {

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

        static constexpr float cFramePerSeconds{ 10.f };
        static constexpr float cFrameDelaySeconds{ 1.f / cFramePerSeconds };

        bool gameOver{};
        float timePool{}, now{};

        Trees trees;
        Robots robots;

        xx::Task<> Logic = Logic_();
        xx::Task<> Logic_();

        void FillData();

        void Update(float delta);

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

}
