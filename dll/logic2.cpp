#include "dll.h"
#include "logic2.h"

namespace Logic2 {

    int Scene::InitBuf(void* buf, int cap) {
        if (!buf || cap <= 0 || drawBuf || drawBufCap) {
            return -__LINE__;
        }
        drawBuf = (XY*)buf;
        drawBufCap = cap;
        return 0;
    }

    int Scene::InitFrameDelay(float v) {
        if (v <= 0) {
            return -__LINE__;
        }
        frameDelay = v;
        return 0;
    }

    int Scene::Begin() {
        if (!drawBuf || frameDelay <= 0) {
            return -__LINE__;
        }

        monsters.Clear();
        monsters.Reserve(drawBufCap);
        for (int i = 0; i < drawBufCap; i++) {
            monsters.Emplace().RndPos();
        }

        return 0;
    }

    xx::Task<> Scene::UpdateCore_() {
        while (true) {
            monsters.ForeachFlags([this](Monster& m)->void {
                m.RndPos();
            });
            co_yield 0;
        }
    }

    int Scene::Update(float delta) {
        timePool += delta;
        while (timePool >= frameDelay) {
            timePool -= frameDelay;

            UpdateCore();
        }
        return 0;
    }

    int Scene::End() {

        monsters.Clear();

        return 0;
    }

    int Scene::Draw() {
        if (monsters.Count() > drawBufCap) {
            return -__LINE__;
        }

        int idx{};
        monsters.ForeachFlags([&](Monster& m)->void {
            memcpy(drawBuf + idx, &m.pos, sizeof(XY));
            ++idx;
        });

        return idx;
    }

    void Monster::RndPos() {
        pos.x = gScene->rnd.Next<float>(-9.f, 9.f);
        pos.y = gScene->rnd.Next<float>(-5.5f, 5.5f);
    }

}
