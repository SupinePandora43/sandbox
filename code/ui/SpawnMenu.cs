
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Reflection.Metadata;
using System.Threading.Tasks;

[Library]
public partial class SpawnMenu : Panel
{
	public static SpawnMenu Instance;
	public Panel ToolPanel { get; private set; }
	public ButtonGroup SpawnMenuLeftTabs;
	public Panel SpawnMenuLeftBody;

	public bool IgnoreMenuButton = false;
	private bool IsOpen = false;

	public SpawnMenu()
	{
		Instance = this;

		StyleSheet.Load( "/ui/SpawnMenu.scss" );

		var left = Add.Panel( "left" );
		{
			var tabs = left.AddChild<ButtonGroup>();
			tabs.AddClass( "tabs" );
			SpawnMenuLeftTabs = tabs;

			var body = left.Add.Panel( "body" );
			SpawnMenuLeftBody = body;
			{
				var props = body.AddChild<SpawnList>();
				tabs.SelectedButton = tabs.AddButtonActive( "Props", ( b ) => props.SetClass( "active", b ) );

				var ents = body.AddChild<EntityList>();
				tabs.AddButtonActive( "Entities", ( b ) => ents.SetClass( "active", b ) );
			}
		}

		var right = Add.Panel( "right" );
		{
			var tabs = right.Add.Panel( "tabs" );
			{
				tabs.Add.Button( "Tools" ).AddClass( "active" );
				tabs.Add.Button( "Utility" );
			}
			var body = right.Add.Panel( "body" );
			{
				var list = body.Add.Panel( "toollist" );
				{
					foreach ( var entry in Library.GetAllAttributes<Sandbox.Tools.BaseTool>() ) {
						if ( entry.Title.StartsWith( "Base" ) )
							continue;

						var button = list.Add.Button( entry.Title );
						button.SetClass( "active", entry.Name == ConsoleSystem.GetValue( "tool_current" ) );

						button.AddEventListener( "onclick", () => {
							ConsoleSystem.Run( "tool_current", entry.Name );
							ConsoleSystem.Run( "inventory_current", "weapon_tool" );

							foreach ( var child in list.Children )
								child.SetClass( "active", child == button );
							ToolPanel.DeleteChildren( true );
						} );
					}
				}
				ToolPanel = body.Add.Panel( "inspector" );
			}
		}

	}

	private bool menuWasPressed = false;
	public override void Tick()
	{
		base.Tick();

		if ( !IgnoreMenuButton ) {
			if ( Input.Pressed( InputButton.Menu ) ) {
				IsOpen = true;
			}
			if ( menuWasPressed && !Input.Down( InputButton.Menu ) ) {
				IsOpen = false;
			}
		}
		menuWasPressed = Input.Down( InputButton.Menu ); // somehow Input.Released wasn't working consistently, so lets emulate it

		Parent.SetClass( "spawnmenuopen", IsOpen );
	}
}
