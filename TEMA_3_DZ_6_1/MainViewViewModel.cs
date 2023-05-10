using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.UI;
using Prism.Commands;
using TEMA_3_DZ_6_1_Library;

namespace TEMA_3_DZ_6_1
{
    public class MainViewViewModel
    {
        private ExternalCommandData _commandData;

        public List<DuctType> DuctTypes { get; } = new List<DuctType>();
        public List<Level> Levels { get; } = new List<Level>();
        public DelegateCommand SaveCommand { get; }
        public List<XYZ> Points { get; } = new List<XYZ>();
        public MEPSystemType MEPSys { get; }
        public DuctType SelectedDuctType { get; set; }
        public Level SelectedLevel { get; set; }
        public double MidElevation { get; set; }


        public MainViewViewModel(ExternalCommandData commandData)
        {
            _commandData = commandData;
            DuctTypes = DuctUtils.GetDuctTypes(commandData);
            Levels = LevelsUtils.GetLevels(commandData);
            SaveCommand = new DelegateCommand(OnSaveCommand);
            MidElevation = 10;
            Points = SelectionUtils.GetPoints(_commandData, "Выберите точки", ObjectSnapTypes.Endpoints);
            MEPSys = SystemTypeUtils.GetMEPSystemTypes(commandData);
        }

        private void OnSaveCommand()
        {
            UIApplication uiapp = _commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            if (Points.Count < 2 || SelectedDuctType == null || SelectedLevel == null)
                return;

            var points = new List<XYZ>();
            for (int i = 0; i < Points.Count; i++)
            {
                if (i == 0)
                    continue;
                var prevPoint = Points[i - 1];
                var currentPoint = Points[i];
                points.Add(prevPoint);
                points.Add(currentPoint);
            }

            using (var ts = new Transaction(doc, "Create duct"))
            {
                ts.Start();
                Duct.Create(doc, MEPSys.Id, SelectedDuctType.Id, SelectedLevel.Id, points[0], points[1])
                    .get_Parameter(BuiltInParameter.RBS_OFFSET_PARAM)
                    .Set(UnitUtils.ConvertFromInternalUnits(MidElevation, UnitTypeId.Meters));

                ts.Commit();
            }
            RaiseCloseRequest();
        }

        public event EventHandler CloseRequest;
        private void RaiseCloseRequest()
        {
            CloseRequest?.Invoke(this, EventArgs.Empty);
        }
    }
}