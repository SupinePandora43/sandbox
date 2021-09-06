
using System.Threading.Tasks;
using Sandbox;
using MinimalExtended;
using System;

namespace SandboxGame
{
	[Library( "sandbox" )]
	public partial class SandboxGame : SandboxAddon, IAutoload
	{
		private SandboxHud _sandboxHud;

		public bool ReloadOnHotload { get; } = false;

		[Event( "hotloaded" )]
		public async void hotload()
		{
			if ( IsServer ) {
				_sandboxHud?.Delete();
				await Task.Delay( 500 ); // gotta wait for clients to hotreload too
				Log.Info( "SandboxPlus: hotloading SandboxHud" );
				_sandboxHud = new SandboxHud();
			}
		}

		public SandboxGame()
		{
			Log.Info( "Init Sandbox" );
			if ( IsServer ) {
				Log.Info( "[Server] initting HUD" );
				// Create the HUD
				_sandboxHud = new SandboxHud();
			}
		}
		~SandboxGame()
		{
			_sandboxHud?.Delete();
		}

		public override void Dispose()
		{
			_sandboxHud?.Delete();
			base.Dispose();
		}

		[Event( "client.join" )]
		public void ClientJoined( Client cl )
		{
			Log.Info( "Client Joined - Spawned" );
			Log.Info( $"{cl.GetType()}" );
			var player = new SandboxPlayer();
			player.Respawn();

			cl.Pawn = player;
		}

		[ServerCmd( "spawn" )]
		public static void Spawn( string modelname )
		{
			var owner = ConsoleSystem.Caller?.Pawn;

			if ( ConsoleSystem.Caller == null )
				return;

			var tr = Trace.Ray( owner.EyePos, owner.EyePos + owner.EyeRot.Forward * 500 )
			  .UseHitboxes()
			  .Ignore( owner )
			  .Run();

			var ent = new Prop();
			ent.Position = tr.EndPos;
			ent.Rotation = Rotation.From( new Angles( 0, owner.EyeRot.Angles().yaw, 0 ) ) * Rotation.FromAxis( Vector3.Up, 180 );
			ent.SetModel( modelname );
			ent.Position = tr.EndPos - Vector3.Up * ent.CollisionBounds.Mins.z;
			Sandbox.Hooks.Entities.TriggerOnSpawned( ent, owner );
		}

		[ServerCmd( "spawn_entity" )]
		public static void SpawnEntity( string entName )
		{
			var owner = ConsoleSystem.Caller.Pawn;

			if ( owner == null )
				return;

			var attribute = Library.GetAttribute( entName );

			if ( attribute == null || !attribute.Spawnable )
				return;

			var tr = Trace.Ray( owner.EyePos, owner.EyePos + owner.EyeRot.Forward * 200 )
			  .UseHitboxes()
			  .Ignore( owner )
			  .Size( 2 )
			  .Run();

			var ent = Library.Create<Entity>( entName );
			if ( ent is BaseCarriable && owner.Inventory != null ) {
				if ( owner.Inventory.Add( ent, true ) )
					return;
			}

			ent.Position = tr.EndPos;
			ent.Rotation = Rotation.From( new Angles( 0, owner.EyeRot.Angles().yaw, 0 ) );

			Sandbox.Hooks.Entities.TriggerOnSpawned( ent, owner );
		}
	}
}

namespace Sandbox.Hooks
{
	public static partial class Undos
	{
		public static event Action<Func<string>, Entity> OnAddUndo;

		// Add an "Undoable" lambda. Should return the string to show in the toast,
		// or empty string if the undoable is redundant and should be skipped over (eg. if the weld was already removed)
		public static void AddUndo( Func<string> undo, Entity owner )
		{
			OnAddUndo?.Invoke( undo, owner );
		}
	}

	public static partial class Entities
	{
		public static event Action<Entity, Entity> OnSpawned;

		public static void TriggerOnSpawned( Entity spawned, Entity owner )
		{
			OnSpawned?.Invoke( spawned, owner );
		}
	}
}
