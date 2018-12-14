using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class SlideOnPress : SlidingObject, IButtonEffected {
	public ButtonLocation ActedUponBy { get; set; }

	public override void LateUpdate() {
		float d = 0;

		bool bottomRight = ActedUponBy.HasFlag(ButtonLocation.BottomRight);
		bool topRight = ActedUponBy.HasFlag(ButtonLocation.TopRight);
		bool topLeft = ActedUponBy.HasFlag(ButtonLocation.TopLeft);
		bool bottomLeft = ActedUponBy.HasFlag(ButtonLocation.BottomLeft);
		if ((topRight || topLeft) && !(bottomRight || bottomLeft)) {
			d = 1;
		}
		else if (!(topRight || topLeft) && (bottomRight || bottomLeft)) {
			d = -1;
		}

		Move(Speed * d * Time.deltaTime);
		ActedUponBy = 0;
	}
}

