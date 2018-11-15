namespace Sabrina.Models
{
    class DungeonVariableExtension
    {
        public enum VariableType
        {
            None,

            Text_Enter = 1,
            Text_Greeting,
            Text_Main,
            Text_Success,
            Text_Failure,
            Text_Dismissal,

            RandomStrokes = 100,
            RandomEdges,
            RandomFlicks,
            RandomSlaps,

            Content = 200,


        }
    }
}
