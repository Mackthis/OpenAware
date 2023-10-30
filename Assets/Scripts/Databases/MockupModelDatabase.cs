using System.Collections.Generic;
using System.Linq;
using ModelLists;

namespace Databases
{
    public class MockupModelDatabase : IModelDatabase
    {
        readonly List<Model> ModelTable;

        public MockupModelDatabase()
        {
            ModelTable = new()
            {
                // All distances are 'placeholder = 0', except for real-life heights (meters)
                new Model( 1, ResourceType.Water, @"cube",                  0, 0, 0, 0, 0),
                new Model( 2, ResourceType.Animal,@"Animals/CowBlW",        0, 0, 0, 0, 0),
                new Model( 3, ResourceType.Animal,@"Animals/ChickenBrown",  0, 0, 0.5f, 0, 0), 
                new Model( 4, ResourceType.Animal,@"Animals/Pig",           0, 0, 0.94f, 0, 0),       
                new Model( 5, ResourceType.Animal,@"Animals/DuckWhite",     0, 0, 0.39f, 0, 0), 
                new Model( 6, ResourceType.Plant, @"Crops/grap",            0, 0, 0, 0, 0),
                new Model( 7, ResourceType.Plant, @"Crops/wheat1",          0, 0, 2.25f, 0, 0),
            };
        }

        public Model GetModel(int id)
        {
            return ModelTable.First(x => x.ID == id);
        }

        public List<Model> GetModels(List<int> ids)
        {
            throw new System.Exception("method not implemented");
        }
    }
}

