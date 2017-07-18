﻿using System;

namespace LiteDB.Shell.Commands
{
    internal class CollectionCount : BaseCollection, ICommand
    {
        public DataAccess Access { get { return DataAccess.Read; } }

        public bool IsCommand(StringScanner s)
        {
            return this.IsCollectionCommand(s, "count");
        }

        public void Execute(LiteEngine engine, StringScanner s, Display display, InputCommand input, Env env)
        {
            var col = this.ReadCollection(engine, s);
            var query = this.ReadQuery(s);

            display.WriteResult(engine.Count(col, query));
        }
    }
}