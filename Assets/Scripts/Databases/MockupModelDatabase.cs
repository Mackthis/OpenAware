using System.Collections.Generic;
using System.Linq;
using IngredientPipeLine;

namespace Databases
{
    public class MockupModelDatabase : IModelDatabase
    {
        readonly List<Model> modelTable;

        public MockupModelDatabase()
        {
            modelTable = new()
            {
                // All distances are 'placeholder = 0', except for real-life heights (meters)
                new Model( 1, ResourceType.Water,  @"Other/Water_bottle",    0, 0, 0.3f, 0, 0),
                new Model( 2, ResourceType.Animal, @"Animals/CowBlW",        0, 0, 0, 0, 0),
                new Model( 3, ResourceType.Animal, @"Animals/ChickenBrown",  0, 0, 0.5f, 0, 0), 
                new Model( 4, ResourceType.Animal, @"Animals/Pig",           0, 0, 0.94f, 0, 0),       
                new Model( 5, ResourceType.Animal, @"Animals/DuckWhite",     0, 0, 0.39f, 0, 0), 
                new Model( 6, ResourceType.Plant,  @"Crops/grap",            0, 0, 1f, 0, 0),
                new Model( 7, ResourceType.Plant,  @"Crops/wheat1",          0, 0, 1.2f, 0, 0),
                new Model( 8, ResourceType.Animal, @"Other/Milk_carton",     0, 0, 0.2f, 0, 0),
                new Model( 9, ResourceType.Plant,  @"Crops/potato",          0, 0, 0.4f, 0, 0),
                new Model(10, ResourceType.Plant,  @"Crops/beet", 0, 0, 0.2f, 0, 0)
            };
        }

        public Model GetModel(int id)
        {
            return modelTable.First(x => x.ID == id);
        }

        public List<Model> GetModels(IEnumerable<int> ids)
        {
            IEnumerable<Model> models =
                from id in ids
                join model in modelTable on id equals model.ID
                select model;
            return models.ToList();
        }
    }
}

