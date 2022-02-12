using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreationModelPlugin
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class CreationModel : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;


            List<Level> levelList = CreateLevelList(doc);
            Level level1 = LevelByName(levelList, "Level 1");
            Level level2 = LevelByName(levelList, "Level 2");

            double width = UnitUtils.ConvertToInternalUnits(10000, UnitTypeId.Millimeters);
            double depth = UnitUtils.ConvertToInternalUnits(5000, UnitTypeId.Millimeters);
            List<XYZ> points = CreateClosedRectangleCurve(width, depth);

            Transaction transaction = new Transaction(doc);
            transaction.Start("Create Wall");

            List<Wall> walls = CreateWalls(doc, points, level1, level2);

            transaction.Commit();
            return Result.Succeeded;
        }


        public List<Level> CreateLevelList(Document doc)
        {
            List<Level> levelList = new FilteredElementCollector(doc)
                                    .OfClass(typeof(Level))
                                    .OfType<Level>()
                                    .ToList();
            return levelList;
        }

        public Level LevelByName(List<Level> levelList, string name)
        {
            Level levelByName = levelList
                                .Where(x => x.Name.Equals(name))
                                .FirstOrDefault();
            return levelByName;
        }

        public List<XYZ> CreateClosedRectangleCurve(double x, double y)
        {
            List<XYZ> rectangle = new List<XYZ>();
            rectangle.Add(new XYZ(-x / 2, -y / 2, 0));
            rectangle.Add(new XYZ(x / 2, -y / 2, 0));
            rectangle.Add(new XYZ(x / 2, y / 2, 0));
            rectangle.Add(new XYZ(-x / 2, y / 2, 0));
            rectangle.Add(new XYZ(-x / 2, -y / 2, 0));

            return rectangle;
        }

        public List<Wall> CreateWalls(Document doc, List<XYZ> closedCurve, Level baseLevel, Level upperLevel)
        {
            List<Wall> walls = new List<Wall>();
            for (int i = 0; i < closedCurve.Count() - 1; i++)
            {
                Line line = Line.CreateBound(closedCurve[i], closedCurve[i + 1]);
                Wall wall = Wall.Create(doc, line, baseLevel.Id, false);
                wall.get_Parameter(BuiltInParameter.WALL_HEIGHT_TYPE).Set(upperLevel.Id);
                walls.Add(wall);
            }
            return walls;
        }

    }
}
