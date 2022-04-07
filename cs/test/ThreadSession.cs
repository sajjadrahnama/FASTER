﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Threading;
using FASTER.core;

namespace FASTER.test.statemachine
{
    internal static class Extension
    {
        public static ThreadSession<K, V, I, O, C, F>
            CreateThreadSession<K, V, I, O, C, F>
            (this FasterKV<K, V>.ClientSessionBuilder<I,O,C> fht, F f)
            where K : new() where V : new() where F : IFunctions<K, V, I, O, C>
        {
            return new ThreadSession<K, V, I, O, C, F>(fht, f);
        }
    }

    internal class ThreadSession<K,V,I,O,C,F>
        where K : new() where V : new() where F : IFunctions<K,V,I,O,C>
    {
        readonly FasterKV<K, V>.ClientSessionBuilder<I, O, C> fht;
        ClientSession<K, V, I, O, C, F, DefaultStoreFunctions<K, V>> s2;
        UnsafeContext<K, V, I, O, C, F, DefaultStoreFunctions<K, V>> uc2;
        readonly F f;
        readonly AutoResetEvent ev = new(false);
        readonly AsyncQueue<string> q = new();

        public ThreadSession(FasterKV<K, V>.ClientSessionBuilder<I, O, C> fht, F f)
        {
            this.fht = fht;
            this.f = f;
            var ss = new Thread(() => SecondSession());
            ss.Start();
            ev.WaitOne();
        }

        public void Refresh()
        {
            OtherSession("refresh");
        }

        public void Dispose()
        {
            OtherSession("dispose");
        }

        private void SecondSession()
        {
            s2 = fht.NewSession(f, null);
            uc2 = s2.GetUnsafeContext();
            uc2.ResumeThread();

            ev.Set();

            while (true)
            {
                var cmd = q.DequeueAsync().Result;
                switch (cmd)
                {
                    case "refresh":
                        uc2.Refresh();
                        ev.Set();
                        break;
                    case "dispose":
                        uc2.SuspendThread();
                        uc2.Dispose();
                        s2.Dispose();
                        ev.Set();
                        return;
                    default:
                        throw new Exception("Unsupported command");
                }
            }
        }

        private void OtherSession(string command)
        {
            q.Enqueue(command);
            ev.WaitOne();
        }
    }
}
