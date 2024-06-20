#include "dll.h"
#include "logic.h"

extern "C" {
    int XX_DLL_EXPORT Init() {
        if (gScene) {
            delete gScene;
        }
        gScene = new Scene();
        return 0;
    }

    // todo
}

Scene* gScene{};
