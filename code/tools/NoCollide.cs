namespace Sandbox.Tools
{
	[Library( "tool_nocollide", Title = "NoCollide", Description = "Disable collisions" )]
	public partial class NoCollideTool : BaseTool
	{
		private ModelEntity entity;

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

				if ( !entity.IsValid() )
				{
					entity = (ModelEntity)tr.Entity;
				}
				else if ( tr.Entity is ModelEntity modelEntity )
				{
					if ( entity == modelEntity )
					{
						entity = null;
						return;
					}

					PhysicsJoint.Generic
						.From( entity.IsWorld ? PhysicsWorld.WorldBody : entity.PhysicsBody )
						.To( modelEntity.IsWorld ? PhysicsWorld.WorldBody : modelEntity.PhysicsBody )
						.Create();

					entity.PhysicsGroup?.Wake();
					modelEntity.PhysicsGroup?.Wake();

					entity = null;
				}
				else return;

				CreateHitEffects( tr.EndPos );
			}
		}
	}
}
