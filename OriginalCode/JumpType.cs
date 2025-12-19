using System;
using UnityEngine;

// Token: 0x02000066 RID: 102
[CreateAssetMenu(menuName = "GrimbartTales/Platformer2D/CharacterController/JumpType")]
public class JumpType : ScriptableObject
{
	// Token: 0x04000242 RID: 578
	public float velocity;

	// Token: 0x04000243 RID: 579
	public bool relativeToForwardDirection;

	// Token: 0x04000244 RID: 580
	public Vector2 relativeDirectionOffset;

	// Token: 0x04000245 RID: 581
	[Range(0f, 1f)]
	public float relativenessWeight = 1f;

	// Token: 0x04000246 RID: 582
	public bool flipCharacterDirectionAfterJump;

	// Token: 0x04000247 RID: 583
	public JumpType.AxisType additiveAxis;

	// Token: 0x04000248 RID: 584
	public bool moveCharacterAwayFromGround;

	// Token: 0x04000249 RID: 585
	public Vector2 moveCharacterAwayFromGroundOffset;

	// Token: 0x0400024A RID: 586
	public float moveCharacterAwayFromGroundParentVelocityFactor = 1f;

	// Token: 0x0400024B RID: 587
	public bool moveCharacterAwayFromGroundRelativeToGround;

	// Token: 0x0400024C RID: 588
	public bool ignoreFacingDirection;

	// Token: 0x0200046E RID: 1134
	public enum AxisType
	{
		// Token: 0x04001A8F RID: 6799
		both,
		// Token: 0x04001A90 RID: 6800
		xAxis,
		// Token: 0x04001A91 RID: 6801
		yAxis,
		// Token: 0x04001A92 RID: 6802
		none
	}
}
