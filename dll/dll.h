#pragma once
#include <xx_task.h>
#include <xx_xy.h>
#include <xx_rnd.h>

using XY = xx::XY;

namespace Logic2 {
    struct Scene;
}

// singleton
extern Logic2::Scene* gScene;

extern "C" {
    int XX_DLL_EXPORT New();

    int XX_DLL_EXPORT InitBuf(void* buf, int cap);
    int XX_DLL_EXPORT InitFrameDelay(float v);

    int XX_DLL_EXPORT Begin();
    int XX_DLL_EXPORT Update(float delta);
    int XX_DLL_EXPORT Draw();
    int XX_DLL_EXPORT End();

    int XX_DLL_EXPORT Delete();
}
