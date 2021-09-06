using Sandbox.UI.Tests;
using Sandbox.UI.Construct;

namespace Sandbox.UI
{
	[Library]
	public partial class MaterialSelector : Panel
	{
		VirtualScrollPanel Canvas;

		public MaterialSelector()
		{
			AddClass( "modelselector" );
			AddClass( "active" );
			AddChild( out Canvas, "canvas" );

			Canvas.Layout.AutoColumns = true;
			Canvas.Layout.ItemSize = new Vector2( 64, 64 );
			Canvas.OnCreateCell = ( cell, data ) => {
				var file = (string)data;
				var material = Material.Load( file );

				// this is inefficient/bad, but I can't find another way to show a material in UI...
				ScenePanel panel;
				using ( SceneWorld.SetCurrent( new SceneWorld() ) ) {
					var mod = SceneObject.CreateModel( "models/maya_testcube_100.vmdl", Transform.Zero );
					mod.SetMaterialOverride( material );

					Light.Point( Vector3.Up * 150.0f, 300.0f, Color.White * 30.0f );

					panel = cell.Add.ScenePanel( SceneWorld.Current, Vector3.Up * 220, new Angles( 90, 0, 0 ).ToRotation(), 45, "icon" );
					panel.RenderOnce = true;
				}

				panel.AddEventListener( "onclick", () => {
					var currentTool = ConsoleSystem.GetValue( "tool_current" );
					ConsoleSystem.Run( $"{currentTool}_material", file );
				} );
			};

			var spawnList = ModelSelector.GetSpawnList( "material" );

			foreach ( var file in spawnList ) {
				if ( !FileSystem.Mounted.FileExists( file + "_c" ) ) {
					continue;
				}
				Canvas.AddItem( file );
			}
		}
	}
}
