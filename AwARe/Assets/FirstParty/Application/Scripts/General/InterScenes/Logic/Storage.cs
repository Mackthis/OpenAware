// /*                                                                                       *\
//     This program has been developed by students from the bachelor Computer Science at
//     Utrecht University within the Software Project course.
//
//     (c) Copyright Utrecht University (Department of Information and Computing Sciences)
// \*                                                                                       */

using AwARe.Data.Logic;
using AwARe.RoomScan.Path;
using Ingredients = AwARe.IngredientList.Logic;

namespace AwARe.InterScenes.Logic
{
    /// <summary>
    /// The in-between-scenes stored data.
    /// </summary>
    public class Storage : IStorage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Storage"/> class.
        /// </summary>
        public Storage() { }

        /// <inheritdoc/>
        public Ingredients.IngredientList ActiveIngredientList { get; set; }
        
        /// <inheritdoc/>
        public Room ActiveRoom { get; set; }

        /// <inheritdoc/>
        public PathData ActivePath { get; set; }
    }

}