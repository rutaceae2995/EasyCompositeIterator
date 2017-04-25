using System;
using System.Collections.Generic;
using System.Linq;
using EasyCompositeIterator;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest
{
    /// <summary>
    /// Unit test class.
    /// </summary>
    [TestClass]
    public sealed class UnitTest
    {
        /// <summary>
        /// Test iterator.
        /// </summary>
        private CompositeIterator<Component> testIterator;

        /// <summary>
        /// Dynamic access test.
        /// </summary>
        [TestMethod]
        [TestCategory(nameof(CompositeIterator<Composite>.AsDynamic))]
        public void DynamicAccessForTopLevelElement()
        {
            var root = this.testIterator.AsDynamic();
            AssertEx.Is(root.ELM1, 101);
            AssertEx.Is(root.ELM2, 102);
            AssertEx.Is(root.ELM3, 103);
            AssertEx.Is(root.ELM4, 104);
            AssertEx.Is(root.ELM5, 105);
            AssertEx.Is(root.ROOT == null, true);
        }

        /// <summary>
        /// Invalid dynamic access test.
        /// </summary>
        [TestMethod]
        [TestCategory(nameof(CompositeIterator<Composite>.AsDynamic))]
        public void DynamicAccessToUnknown()
        {
            var root = this.testIterator.AsDynamic();
            AssertEx.Throws<RuntimeBinderException>(
                () =>
                {
                    var hoge = root.HOGE;
                });
        }

        /// <summary>
        /// Dynamic access test.
        /// </summary>
        [TestMethod]
        [TestCategory(nameof(CompositeIterator<Composite>.AsDynamic))]
        public void DynamicAccessTest()
        {
            this.testIterator.SingleChild(
                    "ROOT",
                    root =>
                    {
                        root.SingleChild(
                            "DSID",
                            dsid =>
                            {
                                // - RCNM (1u)
                                // - RCID (2u)
                                // - ENSP (3u)
                                // - ENED (4u)
                                // - PRED ("test string")
                                var d = dsid.AsDynamic();
                                AssertEx.Is(d.RCNM, 1u);
                                AssertEx.Is(d.RCID, 2u);
                                AssertEx.Is(d.ENSP, 3u);
                                AssertEx.Is(d.ENED, 4u);
                                AssertEx.Is(d.PRED, "test string");
                                return true;
                            });
                        return true;
                    });
        }

        /// <summary>
        /// Tests GetValue method.
        /// </summary>
        [TestMethod]
        [TestCategory(nameof(CompositeIterator<Composite>.GetValue))]
        public void GetValueTest()
        {
            this.testIterator.GetValue<int>("ELM1").Is(101);
            this.testIterator.GetValue<int>("ELM2").Is(102);
            this.testIterator.GetValue<int>("ELM3").Is(103);
            this.testIterator.GetValue<int>("ELM4").Is(104);
            this.testIterator.GetValue<int>("ELM5").Is(105);
        }

        /// <summary>
        /// Tests GetValue method for invalid element name.
        /// </summary>
        [TestMethod]
        [TestCategory(nameof(CompositeIterator<Composite>.GetValue))]
        public void InvalidGetValueTest()
        {
            AssertEx.Throws<InvalidOperationException>(() => this.testIterator.GetValue<int>("HOGE"));
            AssertEx.Throws<InvalidOperationException>(() => this.testIterator.GetValue<int>(null));
            AssertEx.Throws<InvalidOperationException>(() => this.testIterator.GetValue<int>(string.Empty));
        }

        /// <summary>
        /// Tests GetValue method for invalid type casting.
        /// </summary>
        [TestMethod]
        [TestCategory(nameof(CompositeIterator<Composite>.GetValue))]
        public void InvalidGetValueTypeTest()
        {
            AssertEx.Throws<InvalidCastException>(() => this.testIterator.GetValue<double>("ELM1"));
            AssertEx.Throws<InvalidCastException>(() => this.testIterator.GetValue<string>("ELM1"));
            AssertEx.Throws<InvalidCastException>(() => this.testIterator.GetValue<decimal>("ELM1"));
        }

        /// <summary>
        /// Tests GetValues method.
        /// </summary>
        [TestMethod]
        [TestCategory(nameof(CompositeIterator<Composite>.GetValues))]
        public void GetValuesTest()
        {
            this.testIterator.GetValues<int>("Repeat").Is(new[] { 1, 2, 3, 4, 5, 6 });
            this.testIterator.GetValues<int>("ELM1").Is(new[] { 101 });
        }

        /// <summary>
        /// Tests GetValues method.
        /// </summary>
        [TestMethod]
        [TestCategory(nameof(CompositeIterator<Composite>.GetValues))]
        public void InvalidGetValuesTypeTest()
        {
            AssertEx.Throws<InvalidCastException>(() => this.testIterator.GetValues<string>("Repeat"));
            AssertEx.Throws<InvalidCastException>(() => this.testIterator.GetValues<uint>("Repeat"));
            AssertEx.Throws<InvalidCastException>(() => this.testIterator.GetValues<decimal>("Repeat"));
        }

        /// <summary>
        /// Tests GetValues method.
        /// </summary>
        [TestMethod]
        [TestCategory(nameof(CompositeIterator<Composite>.SingleChild))]
        public void SingleChildTest()
        {
            var test = this.testIterator.SingleChild(
                "ROOT",
                root =>
                {
                    // DSID -> RCNM (1u)
                    var rcnm = root.SingleChild<uint>(
                        "DSID",
                        iter =>
                        {
                            var dsid = iter.AsDynamic();
                            return dsid.RCNM;
                        });
                    rcnm.Is(1u);

                    // DSID -> PRED ("test string")
                    var pred = root.SingleChild(
                        "DSID",
                        iter =>
                        {
                            var dsid = iter.AsDynamic();
                            return dsid.PRED;
                        });
                    AssertEx.Is(pred, "test string");

                    // PRID -> C2IT -> YCOO (4)
                    var ycoo = root.SingleChild(
                        "PRID",
                        iter =>
                        {
                            return iter.SingleChild("C2IT", subIter => subIter.AsDynamic().YCOO);
                        });
                    AssertEx.Is(ycoo, 4);

                    // 1 + "test string" + 4 => "1test string4"
                    return rcnm + pred + ycoo;
                });

            // Check the return value.
            AssertEx.Is(test, "1test string4");
        }

        /// <summary>
        /// Tests SingleChild method.
        /// </summary>
        [TestMethod]
        [TestCategory(nameof(CompositeIterator<Composite>.SingleChild))]
        public void InvalidSingleChildTest1()
        {
            // Invalid element name.
            AssertEx.Throws<InvalidOperationException>(
                () =>
                {
                    this.testIterator.SingleChild("X", root => true);
                });
            AssertEx.Throws<InvalidOperationException>(
                () =>
                {
                    this.testIterator.SingleChild(null, root => true);
                });
            AssertEx.Throws<InvalidOperationException>(
                () =>
                {
                    this.testIterator.SingleChild(string.Empty, root => true);
                });

            // Element exists but is not composite
            AssertEx.Throws<InvalidOperationException>(
                () =>
                {
                    this.testIterator.SingleChild("ELM1", root => true);
                });
        }

        /// <summary>
        /// Tests SingleChild method.
        /// </summary>
        [TestMethod]
        [TestCategory(nameof(CompositeIterator<Composite>.SingleChild))]
        public void InvalidSingleChildTest2()
        {
            // Element exists but is not composite
            AssertEx.Throws<InvalidOperationException>(
                () =>
                {
                    this.testIterator.SingleChild("ELM1", root => true);
                });
        }

        /// <summary>
        /// Tests GetValues method.
        /// </summary>
        [TestMethod]
        [TestCategory(nameof(CompositeIterator<Composite>.SingleChild))]
        public void InvalidSingleChildTest3()
        {
            this.testIterator.SingleChild(
                "ROOT",
                root =>
                {
                    // Element name is not found.
                    AssertEx.Throws<InvalidOperationException>(
                        () =>
                        {
                            root.SingleChild<uint>(
                                "HOGE",
                                iter =>
                                {
                                    var dsid = iter.AsDynamic();
                                    return dsid.RCNM;
                                });
                        });

                    // Sub element name is not found (Binder exception)
                    AssertEx.Throws<RuntimeBinderException>(
                        () =>
                        {
                            root.SingleChild(
                                "DSID",
                                iter =>
                                {
                                    var dsid = iter.AsDynamic();
                                    return dsid.HOGE; // Not exists.
                                });
                        });
                    return true;
                });
        }

        /// <summary>
        /// Tests Extract method.
        /// </summary>
        [TestMethod]
        [TestCategory(nameof(CompositeIterator<Composite>.Extract))]
        public void ExtractTest()
        {
            this.testIterator.SingleChild(
                "ROOT",
                root =>
                {
                    var d = root.Extract("DSID");
                    d.ElementName.Is("DSID");

                    root.SingleChild(
                        "DSID",
                        dsid =>
                        {
                            var ensp = dsid.Extract("ENSP");
                            ensp.ElementName.Is("ENSP");
                            ensp.Value.Is(3u);

                            var test = dsid.Extract("TEST");
                            test.Operate().Is(4 * 8);
                            return true;
                        });
                    return true;
                });
        }

        /// <summary>
        /// Tests Extract method.
        /// </summary>
        [TestMethod]
        [TestCategory(nameof(CompositeIterator<Composite>.Extract))]
        public void InvalidExtractTest()
        {
            this.testIterator.SingleChild(
                "ROOT",
                root =>
                {
                    root.Extract("HOGE").IsNull();

                    root.SingleChild(
                        "DSID",
                        dsid =>
                        {
                            root.Extract(null).IsNull();
                            root.Extract(string.Empty).IsNull();
                            root.Extract("HOGE").IsNull();
                            return true;
                        });
                    return true;
                });
        }

        /// <summary>
        /// Tests Split method.
        /// </summary>
        [TestMethod]
        [TestCategory(nameof(CompositeIterator<Composite>.Split))]
        public void SplitTest()
        {
            var testResult = this.testIterator.SingleChild(
                "ROOT",
                root =>
                {
                    return root.SingleChild(
                        "DSID",
                        dsid =>
                        {
                            return dsid.SingleChild(
                                "TEST",
                                test =>
                                {
                                    var result = new List<int[]>();
                                    foreach (var t in test.Split(2).Select(e => e.AsDynamic()))
                                    {
                                        var c = new int[2];
                                        c[0] = t.YCOO;
                                        c[1] = t.XCOO;
                                        result.Add(c);
                                    }

                                    return result;
                                });
                        });
                });

            testResult.Count.Is(4);
            testResult[0].Is(1, 2);
            testResult[1].Is(3, 4);
            testResult[2].Is(5, 6);
            testResult[3].Is(7, 8);
        }

        /// <summary>
        /// Initializes the test member.
        /// </summary>
        [TestInitialize]
        public void Initialize()
        {
            /**
             * Test data.
             * 
             * ROOT
             *   - ELM1 (101)
             *   - ELM2 (102)
             *   - ELM3 (103)
             *   - ELM4 (104)
             *   - ELM5 (105)
             *   - Repeat (1)
             *   - Repeat (2)
             *   - Repeat (3)
             *   - Repeat (4)
             *   - Repeat (5)
             *   - Repeat (6)
             *   - DSID
             *       - RCNM (1u)
             *       - RCID (2u)
             *       - ENSP (3u)
             *       - ENED (4u)
             *       - PRED ("test string")
             *       - TEST
             *           - YCOO (1)
             *           - XCOO (2)
             *           - YCOO (3)
             *           - XCOO (4)
             *           - YCOO (5)
             *           - XCOO (6)
             *           - YCOO (7)
             *           - XCOO (8)
             *   - PRID
             *       - C2IT
             *           - YCOO (1)
             *           - XCOO (2)
             *   - MRID
             *       - C3IL
             *           - YCOO (1)
             *           - XCOO (2)
             *           - ZCOO (3)
             *           - YCOO (4)
             *           - XCOO (5)
             *           - ZCOO (6)
             *           - YCOO (7)
             *           - XCOO (8)
             *           - ZCOO (9)
             *           - YCOO (10)
             *           - XCOO (11)
             *           - ZCOO (12)
             *           - YCOO (13)
             *           - XCOO (14)
             *           - ZCOO (15)
             * 
             */
            var dsid = new Composite("DSID");
            dsid.AddChild(new Leaf("RCNM", 1u));
            dsid.AddChild(new Leaf("RCID", 2u));
            dsid.AddChild(new Leaf("ENSP", 3u));
            dsid.AddChild(new Leaf("ENED", 4u));
            dsid.AddChild(new Leaf("PRED", "test string"));

            var test = new Composite("TEST");
            test.AddChild(new Leaf("YCOO", 1));
            test.AddChild(new Leaf("XCOO", 2));
            test.AddChild(new Leaf("YCOO", 3));
            test.AddChild(new Leaf("XCOO", 4));
            test.AddChild(new Leaf("YCOO", 5));
            test.AddChild(new Leaf("XCOO", 6));
            test.AddChild(new Leaf("YCOO", 7));
            test.AddChild(new Leaf("XCOO", 8));
            dsid.AddChild(test);

            var prid = new Composite("PRID");
            var c2it = new Composite("C2IT");
            c2it.AddChild(new Leaf("YCOO", 4));
            c2it.AddChild(new Leaf("XCOO", -4));
            prid.AddChild(c2it);

            var mrid = new Composite("MRID");
            var c3il = new Composite("C3IL");
            c3il.AddChild(new Leaf("YCOO", 1));
            c3il.AddChild(new Leaf("XCOO", 2));
            c3il.AddChild(new Leaf("ZCOO", 3));
            c3il.AddChild(new Leaf("YCOO", 4));
            c3il.AddChild(new Leaf("XCOO", 5));
            c3il.AddChild(new Leaf("ZCOO", 6));
            c3il.AddChild(new Leaf("YCOO", 7));
            c3il.AddChild(new Leaf("XCOO", 8));
            c3il.AddChild(new Leaf("ZCOO", 9));
            c3il.AddChild(new Leaf("YCOO", 10));
            c3il.AddChild(new Leaf("XCOO", 11));
            c3il.AddChild(new Leaf("ZCOO", 12));
            c3il.AddChild(new Leaf("YCOO", 13));
            c3il.AddChild(new Leaf("XCOO", 14));
            c3il.AddChild(new Leaf("ZCOO", 15));
            mrid.AddChild(c3il);

            var testComponent = new Composite("ROOT")
            {
                Children =
                {
                    dsid,
                    prid,
                    mrid,
                    mrid,
                    mrid
                }
            };

            // Initializes iterator.
            this.testIterator = CompositeIterator<Component>.Create(
                new List<Component>
                {
                    testComponent,
                    new Leaf("ELM1", 101),
                    new Leaf("ELM2", 102),
                    new Leaf("ELM3", 103),
                    new Leaf("ELM4", 104),
                    new Leaf("ELM5", 105),
                    new Leaf("Repeat", 1),
                    new Leaf("Repeat", 2),
                    new Leaf("Repeat", 3),
                    new Leaf("Repeat", 4),
                    new Leaf("Repeat", 5),
                    new Leaf("Repeat", 6),
                }, 
                c => c.ElementName,
                c => c.Value,
                c => c.Children);
        }
    }
}