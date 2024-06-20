#pragma once
#include "dll.h"

namespace Logic2 {

	struct Monster {
		XY pos;
	};

	struct Scene {
		xx::XY *drawBuf{};
		int drawBufCap{};

		float timePool{}, frameDelay{};
		xx::Rnd rnd;
		
		xx::BlockLink<Monster> monsters;

		int InitBuf(void* buf, int cap);
		int InitFrameDelay(float v);
		// todo: init seed ?

		int Begin();
		int Update(float delta);
		int Draw();
		int End();
	};

}
