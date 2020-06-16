using System.Collections.Generic;
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
				var sourceSheet = app.GetGenericObject(sourceSheetId);
				var destinationSheet = app.GetGenericObject(destinationSheetId);
				var destinationSheetProps = destinationSheet.Properties.As<SheetProperties>();
				var sourceChildInfos = sourceSheet.GetChildInfos();
				var destinationChildInfos = destinationSheet.GetChildInfos();
				var childIdMap = new Dictionary<string, string>(sourceChildInfos.Zip(destinationChildInfos, (i0, i1) => new KeyValuePair<string,string>(i0.Id, i1.Id)));
				foreach (var destinationCell in destinationSheetProps.Cells)
				{
					destinationCell.Name = childIdMap[destinationCell.Name];
				}

				destinationSheetProps.MetaDef.Title = "Clone";
				destinationSheet.SetProperties(destinationSheetProps);
				app.DoSave();
			}
		}
	}
}
