namespace OrderKit.Analysis
{
    /// <summary>
    /// Call-graph fixture: one five-deep linear chain, and one two-node recursive
    /// cycle, so a call-graph tool has both a depth answer and a cycle answer.
    ///
    ///   EntryPoint -> StageOne -> StageTwo -> StageThree -> StageFour
    ///   Ping &lt;-> Pong          (mutual recursion, depth-bounded)
    /// </summary>
    public static class CallGraphSample
    {
        public static int StageFour(int value) => value + 1;

        public static int StageThree(int value) => StageFour(value) * 2;

        public static int StageTwo(int value) => StageThree(value) - 3;

        /// <summary>StageFour is reached by two distinct paths.</summary>
        public static int StageOne(int value) => StageTwo(value) + StageFour(value);

        public static int EntryPoint(int value = 1) => StageOne(value);

        public static string Ping(int depth) => depth <= 0 ? "ping" : Pong(depth - 1);

        public static string Pong(int depth) => depth <= 0 ? "pong" : Ping(depth - 1);
    }
}
