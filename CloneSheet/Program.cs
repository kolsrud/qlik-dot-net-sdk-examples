using System.Linq;
using Qlik.Engine;
using Qlik.Sense.Client;

namespace CloneSheet
{
	class Program
	{
		static void Main(string[] args)
		{
			var url = "https://<url>";
			var appName = "<appName>";
			var sourceSheetId = "<sheetId>";

			var location = Location.FromUri(url);
			location.AsNtlmUserViaProxy();
			var appId = location.AppWithNameOrDefault(appName);
			using (var app = location.App(appId)) 
			{
				var destinationSheetId = app.CloneGenericObject(sourceSheetId);
				var destinationSheet = app.GetGenericObject(destinationSheetId);
				var destinationSheetProps = destinationSheet.Properties.As<SheetProperties>();
				var childInfos = destinationSheet.GetChildInfos();
				foreach (var (destinationCell, childInfo) in destinationSheetProps.Cells.Zip(childInfos))
				{
					destinationCell.Name = childInfo.Id;
				}

				destinationSheetProps.MetaDef.Title = "Clone";
				destinationSheet.SetProperties(destinationSheetProps);
				app.DoSave();
			}
		}
	}
}
