using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GrimbartTales.Platformer2D.CharacterController
{
	// Token: 0x02000235 RID: 565
	public class JumpComponent : MonoBehaviour
	{
		// Token: 0x06000CC2 RID: 3266 RVA: 0x000455AC File Offset: 0x000437AC
		private IEnumerator ProcessJumps()
		{
			if (this.jumpCommands.Count > 0)
			{
				this.currentJumpType = this.jumpCommands.Dequeue();
				Vector2 groundOffset = Vector2.zero;
				if (this.currentJumpType.moveCharacterAwayFromGround)
				{
					groundOffset = this.currentJumpType.moveCharacterAwayFromGroundOffset;
					if (this.currentJumpType.moveCharacterAwayFromGroundRelativeToGround)
					{
						groundOffset = this.groundedComponent.calculateSlopeAngleRelativeTo.rotation * groundOffset;
					}
					groundOffset.x *= 1f + this.positionParent.CurrentParentSpeed.x * this.currentJumpType.moveCharacterAwayFromGroundParentVelocityFactor;
					groundOffset.y *= 1f + this.positionParent.CurrentParentSpeed.y * this.currentJumpType.moveCharacterAwayFromGroundParentVelocityFactor;
					
					// Convert the position offset to a one-frame velocity impulse
					groundOffset /= Time.fixedDeltaTime;
				}
				Vector2 vector2 = this.currentJumpType.relativeDirectionOffset;
				if (!this.currentJumpType.ignoreFacingDirection && this.character.Direction == CharacterBase.FacingDirection.Left)
				{
					vector2.x *= -1f;
				}
				if (this.currentJumpType.relativeToForwardDirection)
				{
					vector2 = Vector2.Lerp(vector2, this.groundedComponent.calculateSlopeAngleRelativeTo.rotation * vector2, this.currentJumpType.relativenessWeight);
				}
				Vector2 vector3 = vector2.normalized * this.currentJumpType.velocity + groundOffset;
				switch (this.currentJumpType.additiveAxis)
				{
				case JumpType.AxisType.both:
					this.r2d.velocity += vector3;
					break;
				case JumpType.AxisType.xAxis:
					this.r2d.velocity = new Vector2(this.r2d.velocity.x + vector3.x, vector3.y);
					break;
				case JumpType.AxisType.yAxis:
					this.r2d.velocity = new Vector2(vector3.x, this.r2d.velocity.y + vector3.y);
					break;
				case JumpType.AxisType.none:
					this.r2d.velocity = vector3;
					break;
				}
				if (this.currentJumpType.flipCharacterDirectionAfterJump)
				{
					yield return new WaitForEndOfFrame();
					if (this.character.Direction == CharacterBase.FacingDirection.Left)
					{
						this.character.Direction = CharacterBase.FacingDirection.Right;
					}
					else
					{
						this.character.Direction = CharacterBase.FacingDirection.Left;
					}
				}
				if (this.r2d.velocity.x >= 0.5f && this.character.Direction == CharacterBase.FacingDirection.Left)
				{
					yield return new WaitForEndOfFrame();
					this.character.Direction = CharacterBase.FacingDirection.Right;
				}
				else if (this.r2d.velocity.x <= -0.5f && this.character.Direction == CharacterBase.FacingDirection.Right)
				{
					yield return new WaitForEndOfFrame();
					this.character.Direction = CharacterBase.FacingDirection.Left;
				}
			}
			yield break;
		}

		// Token: 0x06000CC3 RID: 3267 RVA: 0x000455BB File Offset: 0x000437BB
		public void TriggerJump(JumpType jumpType)
		{
			this.jumpCommands.Enqueue(jumpType);
			base.StartCoroutine(this.ProcessJumps());
		}

		// Token: 0x06000CC4 RID: 3268 RVA: 0x000455D8 File Offset: 0x000437D8
		public void TriggerNormalJumpFromCurrentJumpPlatform()
		{
			JumpType jumpType = new JumpType();
			jumpType.relativeToForwardDirection = false;
			jumpType.additiveAxis = JumpType.AxisType.xAxis;
			jumpType.velocity = this.lastJumpPlatform.strength;
			jumpType.relativeDirectionOffset = this.lastJumpPlatform.transform.up;
			jumpType.ignoreFacingDirection = true;
			this.currentJumpPlatform = null;
			this.TriggerJump(jumpType);
			base.StartCoroutine(this.ProcessJumps());
		}

		// Token: 0x06000CC5 RID: 3269 RVA: 0x00045648 File Offset: 0x00043848
		public void TriggerLargeJumpFromCurrentJumpPlatform()
		{
			JumpType jumpType = new JumpType();
			jumpType.relativeToForwardDirection = false;
			jumpType.additiveAxis = JumpType.AxisType.xAxis;
			jumpType.velocity = this.lastJumpPlatform.stompStrength;
			jumpType.relativeDirectionOffset = this.lastJumpPlatform.transform.up;
			jumpType.ignoreFacingDirection = true;
			this.currentJumpPlatform = null;
			this.TriggerJump(jumpType);
			base.StartCoroutine(this.ProcessJumps());
		}

		// Token: 0x04000E48 RID: 3656
		private Queue<JumpType> jumpCommands = new Queue<JumpType>();

		// Token: 0x04000E49 RID: 3657
		public GroundedComponent groundedComponent;

		// Token: 0x04000E4A RID: 3658
		public Rigidbody2D r2d;

		// Token: 0x04000E4B RID: 3659
		public Transform directionRelativeToTransform;

		// Token: 0x04000E4C RID: 3660
		public CharacterBase character;

		// Token: 0x04000E4D RID: 3661
		public RigidBody2DPositionParent positionParent;

		// Token: 0x04000E4E RID: 3662
		public JumpType currentJumpType;

		// Token: 0x04000E4F RID: 3663
		public JumpPlatform currentJumpPlatform;

		// Token: 0x04000E50 RID: 3664
		public JumpPlatform lastJumpPlatform;
	}
}
