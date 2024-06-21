#pragma once
#include "dll.h"

namespace Logic2 {

	struct Monster {
		XY pos;
		void RndPos();
	};

	struct Scene {
		XY *drawBuf{};
		int drawBufCap{};

		float timePool{}, frameDelay{};
		xx::Rnd rnd;
		
		xx::BlockLink<Monster> monsters;

		int InitBuf(void* buf, int cap);
		int InitFrameDelay(float v);
		// todo: init seed ?

		int Begin();
		xx::Task<> UpdateCore = UpdateCore_();
		xx::Task<> UpdateCore_();
		int Update(float delta);
		int Draw();
		int End();
	};

}
