using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class InputPackage {
	public float LeftPlayerVertical { get; set; }
	public float RightPlayerVertical { get; set; }

	public bool LeftPlayerStart { get; set; }
	public bool RightPlayerStart { get; set; }

	public bool LeftPlayerBoost { get; set; }
	public bool LeftPlayerCtrl { get; set; }

	public bool RightPlayerBoost { get; set; }
	public bool RightPlayerCtrl { get; set; }
}

public class HouseInputPackage : InputPackage {
	public float LeftPlayerHorizontal { get; set; }
	public float RightPlayerHorizontal { get; set; }
}


