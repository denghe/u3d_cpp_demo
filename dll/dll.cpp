#include "dll.h"
#include "logic1.h"
#include "logic2.h"

extern "C" {

    int XX_DLL_EXPORT New() {
        if (gScene) {
            delete gScene;
            gScene = new Logic2::Scene();
            return __LINE__;
        } else {
            gScene = new Logic2::Scene();
            return 0;
        }
    }

    int XX_DLL_EXPORT InitBuf(void* buf, int cap) {
        if (!gScene) return __LINE__;
        return gScene->InitBuf(buf, cap);
    }

    int XX_DLL_EXPORT InitFrameDelay(float v) {
        if (!gScene) return __LINE__;
        return gScene->InitFrameDelay(v);
    }

    int XX_DLL_EXPORT Begin() {
        if (!gScene) return __LINE__;
        return gScene->Begin();
    }

    int XX_DLL_EXPORT Update(float delta) {
        if (!gScene) return __LINE__;
        return gScene->Update(delta);
    }

    int XX_DLL_EXPORT Draw() {
        if (!gScene) return __LINE__;
        return gScene->Draw();
    }

    int XX_DLL_EXPORT End() {
        if (!gScene) return __LINE__;
        return gScene->End();
    }

    int XX_DLL_EXPORT Delete() {
        if (gScene) {
            delete gScene;
            gScene = {};
            return 0;
        } else {
            return __LINE__;
        }
    }

}

Logic2::Scene* gScene{};
