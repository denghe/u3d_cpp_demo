#include "dll.h"
#include <iostream>


struct Scene {
    static constexpr float fps{ 60 };
    static constexpr float frameDelay{ 1.0f / fps };
    int cap{};
    std::unique_ptr<XY[]> buf;

    Scene() {
        cap = 100000;
        buf = std::make_unique<XY[]>(cap);
    }

    void Run() {
        int r = ::New();
        assert(r == 0);

        r = ::InitBuf(buf.get(), cap);
        assert(r == 0);

        r = ::InitFrameDelay(frameDelay);
        assert(r == 0);

        r = ::Begin();
        assert(r == 0);

        for (int i = 0; i < 10; i++)
        {
            r = ::Update(frameDelay);
            assert(r == 0);

            r = ::Draw();
            assert(r > 0);

            xx::CoutN("draw return ", r);
            for (int j = 0; j < r; j++)
            {
                xx::CoutN(buf[j]);
            }
        }

        r = ::End();
        assert(r == 0);

        r = ::Delete();
        assert(r == 0);
    }
};

int main() {
    Scene{}.Run();
	return 0;
}
