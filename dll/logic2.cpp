#include "dll.h"
#include "logic2.h"

namespace Logic2 {

    int Scene::InitBuf(void* buf, int cap) {
        if (!buf || cap <= 0 || drawBuf || drawBufCap) {
            return -__LINE__;
        }
        drawBuf = (xx::XY*)buf;
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

        for (int i = 0; i < 2; i++) {
            monsters.Emplace().pos = {};
        }

        return 0;
    }

    int Scene::Update(float delta) {
        timePool += delta;
        while (timePool >= frameDelay) {
            timePool -= frameDelay;

            monsters.ForeachFlags([this](Monster& m)->void {
                m.pos.x = rnd.Next<float>(-9.f, 9.f);
                m.pos.y = rnd.Next<float>(-5.5f, 5.5f);
            });

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

}
