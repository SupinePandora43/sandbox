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
				Scene panel;
				using ( SceneWorld.SetCurrent( new SceneWorld() ) ) {
					var mod = SceneObject.CreateModel( "models/maya_testcube_100.vmdl", Transform.Zero );
					mod.SetMaterialOverride( material );

					Light.Point( Vector3.Up * 150.0f, 500.0f, Color.White * 5000.0f );

					panel = cell.Add.Scene( SceneWorld.Current, Vector3.Up * 120, new Angles( 90, 0, 0 ), 45, "icon" );
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
