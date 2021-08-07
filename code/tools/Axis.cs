namespace Sandbox.Tools
{
	[Library( "tool_axis", Title = "Axis", Description = "Physical Axis" )]
	public partial class AxisTool : BaseTool
	{
		private BasePhysics ent1, ent2;

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

				if ( state is State.GET_ATTACHABLE )
				{
					if ( tr.Entity.IsWorld || tr.Entity is WorldEntity ) return;
					if ( !(tr.Body.IsValid() && (tr.Body.PhysicsGroup != null) && tr.Body.Entity.IsValid()) ) return;

					ent1 = (BasePhysics)tr.Entity;

					WNorm1 = tr.Normal;
					LNorm1 = ent1.Transform.NormalToLocal( WNorm1 );

					WPos1 = tr.EndPos;
					LPos1 = ent1.Transform.PointToLocal( WPos1 );

					state = State.GET_ATTACH_TO_POINT;
					CreateHitEffects( tr.EndPos );
				}
				else
				{
					if ( !(tr.Body.IsValid() && (tr.Body.PhysicsGroup != null) && tr.Body.Entity.IsValid())) return;
					
					ent2 = (BasePhysics)tr.Entity;

					if ( !ent1.IsValid() )
					{
						state = State.GET_ATTACHABLE;
						return;
					}

					WNorm1 = ent1.Transform.NormalToWorld( LNorm1 );
					WNorm2 = tr.Normal;
					LNorm2 = ent2.Transform.NormalToLocal( WNorm2 );
					
					WPos1 = ent1.Transform.PointToWorld( LPos1 );
					WPos2 = tr.EndPos;
					LPos2 = ent2.Transform.PointToLocal( WPos2 );

					// fun begins

					// turn origin
					ent1.Rotation = Rotation.LookAt( WNorm2 ) * Rotation.From( new Angles( 0, -180, 0 ) );

					// now turn normal
					//ent1.Rotation = Rotation.Difference(Rotation.LookAt( ent1.Rotation.Forward), Rotation.LookAt( WNorm1 ));

					//ent1.Rotation = Rotation.LookAt( WNorm1 * WNorm2 );
					ent1.LocalRotation = Rotation.Difference( Rotation.LookAt( LNorm1 ), ent1.Rotation );

					// fun ends

					state = State.GET_ATTACHABLE;
				}

				/*var idk = PhysicsJoint.Revolute
					.From( entity.PhysicsBody )
					.To( tr.Body )
					.WithPivot( tr.EndPos )
					.WithBasis( Rotation.LookAt( tr.Normal ) * Rotation.From( new Angles( 90, 0, 0 ) ) )
					.Create();*/
			}
		}
	}
}
