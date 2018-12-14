using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinOnPress : Spinner, IButtonEffected {
	public ButtonLocation ActedUponBy { get; set; }

	public override void LateUpdate() {
		float spin = 0;
		bool bottomRight = ActedUponBy.HasFlag(ButtonLocation.BottomRight);
		bool topRight = ActedUponBy.HasFlag(ButtonLocation.TopRight);
		bool topLeft = ActedUponBy.HasFlag(ButtonLocation.TopLeft);
		bool bottomLeft = ActedUponBy.HasFlag(ButtonLocation.BottomLeft);
		if( (bottomRight || topLeft) && !(topRight || bottomLeft) ) {
			spin = -1;
		}
		else if (!(bottomRight || topLeft) && (topRight || bottomLeft)) {
			spin = 1;
		}

		Spin(spin);
		ActedUponBy = 0;
	}
}
