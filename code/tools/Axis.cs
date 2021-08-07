namespace Sandbox.Tools
{
	[Library( "tool_axis", Title = "Axis", Description = "Physical Axis" )]
	public partial class AxisTool : BaseTool
	{
		private ModelEntity ent1, ent2;

		private Vector3 LNorm1, LNorm2;
		private Vector3 WNorm1, WNorm2;
		private Vector3 LPos1, LPos2;
		private Vector3 WPos1, WPos2;

		private enum State
		{
			GET_ATTACHABLE,
			GET_ATTACH_TO_POINT
		}

		private State state = State.GET_ATTACHABLE;

		public override void Simulate()
		{
			if ( !Host.IsServer )
				return;

			using ( Prediction.Off() )
			{
				if ( !Input.Pressed( InputButton.Attack1 ) )
					return;

				var startPos = Owner.EyePos;
				var dir = Owner.EyeRot.Forward;

				var tr = Trace.Ray( startPos, startPos + dir * MaxTraceDistance )
					.Ignore( Owner )
					.Run();

				if ( !tr.Hit )
					return;

				if ( !tr.Entity.IsValid() )
					return;

				if ( !(tr.Body.IsValid() && (tr.Body.PhysicsGroup != null) && tr.Body.Entity.IsValid()) ) return;
				
				if ( state is State.GET_ATTACHABLE || !ent1.IsValid() )
				{
					if ( tr.Entity.IsWorld || tr.Entity is WorldEntity ) return;

					ent1 = (ModelEntity)tr.Entity;

					WNorm1 = tr.Normal;
					LNorm1 = ent1.Transform.NormalToLocal( WNorm1 );

					WPos1 = tr.EndPos;
					LPos1 = ent1.Transform.PointToLocal( WPos1 );

					state = State.GET_ATTACH_TO_POINT;
				}
				else
				{
					ent2 = (ModelEntity)tr.Entity;

					if ( (!ent1.IsValid()) || ent1 == ent2 )
					{
						state = State.GET_ATTACHABLE;
						return;
					}

					#region Rotation

					WNorm1 = ent1.Transform.NormalToWorld( LNorm1 );
					WNorm2 = tr.Normal;
					LNorm2 = ent2.Transform.NormalToLocal( WNorm2 );

					ent1.Rotation = Rotation.LookAt( WNorm2 ) * Rotation.From( new Angles( 0, -180, 0 ) );
					ent1.LocalRotation = Rotation.Difference( Rotation.LookAt( LNorm1 ), ent1.Rotation );

					#endregion Rotation

					#region Position

					WPos1 = ent1.Transform.PointToWorld( LPos1 );
					WPos2 = tr.EndPos;

					ent1.Position += WPos2 - WPos1;

					#endregion Position

					PhysicsJoint.Revolute
						.From( ent1.IsWorld ? PhysicsWorld.WorldBody : ent1.PhysicsBody )
						.To( ent2.IsWorld ? PhysicsWorld.WorldBody : ent2.PhysicsBody )
						.WithPivot( tr.EndPos )
						.WithBasis( Rotation.LookAt( tr.Normal ) * Rotation.From( new Angles( 90, 0, 0 ) ) )
						.Create();

					ent1.PhysicsGroup?.Wake();
					ent2.PhysicsGroup?.Wake();

					state = State.GET_ATTACHABLE;
				}
				CreateHitEffects( tr.EndPos );
			}
		}
	}
}
