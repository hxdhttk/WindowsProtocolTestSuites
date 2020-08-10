﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Protocols.TestTools;
using Microsoft.Protocols.TestTools.StackSdk.FileAccessService.WSP;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Protocols.TestSuites.WspTS
{
    public partial class CPMCreateQueryTestCases : WspCommonTestBase
    {
        private uint[] hierarchicalCursors;

        #region Test Cases

        [TestMethod]
        [TestCategory("CPMCreateQuery")]
        [Description("This test case is designed to verify the server behavior if a range-based grouping operation over a numeric property is requested in CPMCreateQueryIn.")]
        public void CPMCreateQuery_Categorization_NumericRanges()
        {
            var searchScope = Site.Properties.Get("QueryPath") + "Data/CreateQuery_Categorization";
            var mappedProps = new CFullPropSpec[] { WspConsts.System_Size, WspConsts.System_ItemName };
            var columnIdsForGrouping = new uint[] { 0 };
            var rangePivots = new Dictionary<object, string>[]
            {
                new Dictionary<object, string>
                {
                    [5UL] = "Small",
                    [10UL] = "Medium",
                    [50UL] = "Large"
                },
            };
            var leafResultColumnIds = new uint[] { 0, 1 };
            var createQueryOut = CreateQueryWithCategorization(searchScope, mappedProps, columnIdsForGrouping, rangePivots, leafResultColumnIds);
            hierarchicalCursors = createQueryOut.aCursors;

            var propsForBindings = new Dictionary<uint, CFullPropSpec[]>
            {
                [0] = new CFullPropSpec[] { mappedProps[columnIdsForGrouping[0]] },
                [1] = new CFullPropSpec[] { mappedProps[leafResultColumnIds[0]], mappedProps[leafResultColumnIds[1]] }
            };
            SetBindingsForHierarchicalCursors(propsForBindings);

            var rowStrings = GetChapteredResultsFromRows(1);
            var expectedResults = new string[]
            {
                "0;",
                "    0;abc0.txt;",
                "    4;abc1.txt;",
                "Small;",
                "    8;abc2.txt;",
                "Medium;",
                "    11;abc3.txt;",
                "    12;rst0.txt;",
                "    12;rst2.txt;",
                "    12;rst3.txt;",
                "    30;cda0.txt;",
                "    30;cda1.txt;",
                "    30;cda2.txt;",
                "Large;",
                "    117;ggg4.txt;",
                "    23552;ttt0.doc;",
                "    23552;ttt3.doc;",
                "    27648;mmm0.xls;",
                "    27648;mmm2.xls;",
                "    43520;kkk0.ppt;",
                "    43520;kkk1.ppt;",
                "NULL;",
                "    NULL;AAA;",
                "    NULL;BBB;",
                "    NULL;EEE;",
                "    NULL;SSS;",
            };

            var succeed = expectedResults.SequenceEqual(rowStrings);
            Site.Assert.IsTrue(succeed, "The hierarchical chaptered results should be the same as the expected results.");
        }

        [TestMethod]
        [TestCategory("CPMCreateQuery")]
        [Description("This test case is designed to verify the server behavior if a range-based grouping operation over a string property is requested in CPMCreateQueryIn.")]
        public void CPMCreateQuery_Categorization_UnicodeRanges()
        {
            var searchScope = Site.Properties.Get("QueryPath") + "Data/CreateQuery_Categorization";
            var mappedProps = new CFullPropSpec[] { WspConsts.System_Author, WspConsts.System_ItemName };
            var columnIdsForGrouping = new uint[] { 0 };
            var rangePivots = new Dictionary<object, string>[]
            {
                new Dictionary<object, string>
                {
                    ["d"] = "Range 1",
                    ["m"] = "Range 2",
                    ["r"] = "Range 3"
                },
            };
            var leafResultColumnIds = new uint[] { 0, 1 };
            var createQueryOut = CreateQueryWithCategorization(searchScope, mappedProps, columnIdsForGrouping, rangePivots, leafResultColumnIds);
            hierarchicalCursors = createQueryOut.aCursors;

            var propsForBindings = new Dictionary<uint, CFullPropSpec[]>
            {
                [0] = new CFullPropSpec[] { mappedProps[columnIdsForGrouping[0]] },
                [1] = new CFullPropSpec[] { mappedProps[leafResultColumnIds[0]], mappedProps[leafResultColumnIds[1]] }
            };
            SetBindingsForHierarchicalCursors(propsForBindings);

            var rowStrings = GetChapteredResultsFromRows(1);
            var expectedResults = new string[]
            {
                ";",
                "    [AAA,BBB];ttt0.doc;",
                "    [AAA,CCC];ttt3.doc;",
                "    [ABC,AAA];mmm0.xls;",
                "    [NNN,BBB];kkk0.ppt;",
                "Range 1;",
                "    [GGG];kkk1.ppt;",
                "Range 2;",
                "    [NNN,BBB];kkk0.ppt;",
                "Range 3;",
                "    [SSS];mmm2.xls;",
                "NULL;",
                "    NULL;AAA;",
                "    NULL;abc0.txt;",
                "    NULL;abc1.txt;",
                "    NULL;abc2.txt;",
                "    NULL;abc3.txt;",
                "    NULL;BBB;",
                "    NULL;cda0.txt;",
                "    NULL;cda1.txt;",
                "    NULL;cda2.txt;",
                "    NULL;EEE;",
                "    NULL;ggg4.txt;",
                "    NULL;rst0.txt;",
                "    NULL;rst2.txt;",
                "    NULL;rst3.txt;",
                "    NULL;SSS;",
            };

            var succeed = expectedResults.SequenceEqual(rowStrings);
            Site.Assert.IsTrue(succeed, "The hierarchical chaptered results should be the same as the expected results.");
        }

        [TestMethod]
        [TestCategory("CPMCreateQuery")]
        [Description("This test case is designed to verify the server behavior if a unique grouping operation over a numeric property is requested in CPMCreateQueryIn.")]
        public void CPMCreateQuery_Categorization_UniqueNumericValues()
        {
            var searchScope = Site.Properties.Get("QueryPath") + "Data/CreateQuery_Categorization";
            var mappedProps = new CFullPropSpec[] { WspConsts.System_Size, WspConsts.System_ItemName };
            var columnIdsForGrouping = new uint[] { 0 };
            var rangePivots = new Dictionary<object, string>[] { null };
            var leafResultColumnIds = new uint[] { 0, 1 };
            var createQueryOut = CreateQueryWithCategorization(searchScope, mappedProps, columnIdsForGrouping, rangePivots, leafResultColumnIds);
            hierarchicalCursors = createQueryOut.aCursors;

            var propsForBindings = new Dictionary<uint, CFullPropSpec[]>
            {
                [0] = new CFullPropSpec[] { mappedProps[columnIdsForGrouping[0]] },
                [1] = new CFullPropSpec[] { mappedProps[leafResultColumnIds[0]], mappedProps[leafResultColumnIds[1]] }
            };
            SetBindingsForHierarchicalCursors(propsForBindings);

            var rowStrings = GetChapteredResultsFromRows(1);
            var expectedResults = new string[]
            {
                "0;",
                "    0;abc0.txt;",
                "4;",
                "    4;abc1.txt;",
                "8;",
                "    8;abc2.txt;",
                "11;",
                "    11;abc3.txt;",
                "12;",
                "    12;rst0.txt;",
                "    12;rst2.txt;",
                "    12;rst3.txt;",
                "30;",
                "    30;cda0.txt;",
                "    30;cda1.txt;",
                "    30;cda2.txt;",
                "117;",
                "    117;ggg4.txt;",
                "23552;",
                "    23552;ttt0.doc;",
                "    23552;ttt3.doc;",
                "27648;",
                "    27648;mmm0.xls;",
                "    27648;mmm2.xls;",
                "43520;",
                "    43520;kkk0.ppt;",
                "    43520;kkk1.ppt;",
                "NULL;",
                "    NULL;AAA;",
                "    NULL;BBB;",
                "    NULL;EEE;",
                "    NULL;SSS;",
            };

            var succeed = expectedResults.SequenceEqual(rowStrings);
            Site.Assert.IsTrue(succeed, "The hierarchical chaptered results should be the same as the expected results.");
        }

        [TestMethod]
        [TestCategory("CPMCreateQuery")]
        [Description("This test case is designed to verify the server behavior if a unique grouping operation over a string property is requested in CPMCreateQueryIn.")]
        public void CPMCreateQuery_Categorization_UniqueUnicodeValues()
        {
            var searchScope = Site.Properties.Get("QueryPath") + "Data/CreateQuery_Categorization";
            var mappedProps = new CFullPropSpec[] { WspConsts.System_Author, WspConsts.System_ItemName };
            var columnIdsForGrouping = new uint[] { 0 };
            var rangePivots = new Dictionary<object, string>[] { null };
            var leafResultColumnIds = new uint[] { 0, 1 };
            var createQueryOut = CreateQueryWithCategorization(searchScope, mappedProps, columnIdsForGrouping, rangePivots, leafResultColumnIds);
            hierarchicalCursors = createQueryOut.aCursors;

            var propsForBindings = new Dictionary<uint, CFullPropSpec[]>
            {
                [0] = new CFullPropSpec[] { mappedProps[columnIdsForGrouping[0]] },
                [1] = new CFullPropSpec[] { mappedProps[leafResultColumnIds[0]], mappedProps[leafResultColumnIds[1]] }
            };
            SetBindingsForHierarchicalCursors(propsForBindings);

            var rowStrings = GetChapteredResultsFromRows(1);
            var expectedResults = new string[]
            {
                "AAA;",
                "    [AAA,BBB];ttt0.doc;",
                "    [AAA,CCC];ttt3.doc;",
                "    [ABC,AAA];mmm0.xls;",
                "ABC;",
                "    [ABC,AAA];mmm0.xls;",
                "BBB;",
                "    [AAA,BBB];ttt0.doc;",
                "    [NNN,BBB];kkk0.ppt;",
                "CCC;",
                "    [AAA,CCC];ttt3.doc;",
                "GGG;",
                "    [GGG];kkk1.ppt;",
                "NNN;",
                "    [NNN,BBB];kkk0.ppt;",
                "SSS;",
                "    [SSS];mmm2.xls;",
                "NULL;",
                "    NULL;AAA;",
                "    NULL;abc0.txt;",
                "    NULL;abc1.txt;",
                "    NULL;abc2.txt;",
                "    NULL;abc3.txt;",
                "    NULL;BBB;",
                "    NULL;cda0.txt;",
                "    NULL;cda1.txt;",
                "    NULL;cda2.txt;",
                "    NULL;EEE;",
                "    NULL;ggg4.txt;",
                "    NULL;rst0.txt;",
                "    NULL;rst2.txt;",
                "    NULL;rst3.txt;",
                "    NULL;SSS;",
            };

            var succeed = expectedResults.SequenceEqual(rowStrings);
            Site.Assert.IsTrue(succeed, "The hierarchical chaptered results should be the same as the expected results.");
        }

        [TestMethod]
        [TestCategory("CPMCreateQuery")]
        [Description("This test case is designed to verify the server behavior if a netsted grouping operation over 2 properties is requested in CPMCreateQueryIn.")]
        public void CPMCreateQuery_Categorization_NestedGrouping_2Levels()
        {
            var searchScope = Site.Properties.Get("QueryPath") + "Data/CreateQuery_Categorization";
            var mappedProps = new CFullPropSpec[] { WspConsts.System_ItemFolderNameDisplay, WspConsts.System_Author, WspConsts.System_ItemName };
            var columnIdsForGrouping = new uint[] { 0, 1 };
            var rangePivots = new Dictionary<object, string>[]
            {
                null,
                new Dictionary<object, string>
                {
                    ["d"] = null,
                    ["m"] = null,
                    ["r"] = null
                }
            };
            var leafResultColumnIds = new uint[] { 0, 1, 2 };
            var createQueryOut = CreateQueryWithCategorization(searchScope, mappedProps, columnIdsForGrouping, rangePivots, leafResultColumnIds);
            hierarchicalCursors = createQueryOut.aCursors;

            var propsForBindings = new Dictionary<uint, CFullPropSpec[]>
            {
                [0] = new CFullPropSpec[] { mappedProps[columnIdsForGrouping[0]] },
                [1] = new CFullPropSpec[] { mappedProps[columnIdsForGrouping[1]] },
                [2] = new CFullPropSpec[] { mappedProps[leafResultColumnIds[0]], mappedProps[leafResultColumnIds[1]], mappedProps[leafResultColumnIds[2]] }
            };
            SetBindingsForHierarchicalCursors(propsForBindings);

            var rowStrings = GetChapteredResultsFromRows(2);
            var expectedResults = new string[]
            {
                "AAA;",
                "    d;",
                "        AAA;[GGG];kkk1.ppt;",
                "    NULL;",
                "        AAA;NULL;abc1.txt;",
                "        AAA;NULL;cda1.txt;",
                "BBB;",
                "    r;",
                "        BBB;[SSS];mmm2.xls;",
                "    NULL;",
                "        BBB;NULL;abc2.txt;",
                "        BBB;NULL;cda2.txt;",
                "        BBB;NULL;rst2.txt;",
                "CreateQuery_Categorization;",
                "    ;",
                "        CreateQuery_Categorization;[AAA,BBB];ttt0.doc;",
                "        CreateQuery_Categorization;[ABC,AAA];mmm0.xls;",
                "    m;",
                "        CreateQuery_Categorization;[NNN,BBB];kkk0.ppt;",
                "    NULL;",
                "        CreateQuery_Categorization;NULL;AAA;",
                "        CreateQuery_Categorization;NULL;abc0.txt;",
                "        CreateQuery_Categorization;NULL;BBB;",
                "        CreateQuery_Categorization;NULL;cda0.txt;",
                "        CreateQuery_Categorization;NULL;EEE;",
                "        CreateQuery_Categorization;NULL;rst0.txt;",
                "        CreateQuery_Categorization;NULL;SSS;",
                "EEE;",
                "    ;",
                "        EEE;[AAA,CCC];ttt3.doc;",
                "    NULL;",
                "        EEE;NULL;abc3.txt;",
                "        EEE;NULL;rst3.txt;",
                "SSS;",
                "    NULL;",
                "        SSS;NULL;ggg4.txt;",
            };

            var succeed = expectedResults.SequenceEqual(rowStrings);
            Site.Assert.IsTrue(succeed, "The hierarchical chaptered results should be the same as the expected results.");
        }

        [TestMethod]
        [TestCategory("CPMCreateQuery")]
        [Description("This test case is designed to verify the server behavior if a netsted grouping operation over 3 properties is requested in CPMCreateQueryIn.")]
        public void CPMCreateQuery_Categorization_NestedGrouping_3Levels()
        {
            var searchScope = Site.Properties.Get("QueryPath") + "Data/CreateQuery_Categorization";
            var mappedProps = new CFullPropSpec[] { WspConsts.System_ItemFolderNameDisplay, WspConsts.System_Author, WspConsts.System_Size, WspConsts.System_ItemName };
            var columnIdsForGrouping = new uint[] { 0, 1, 2 };
            var rangePivots = new Dictionary<object, string>[]
            {
                null,
                new Dictionary<object, string>
                {
                    ["d"] = null,
                    ["m"] = null,
                    ["r"] = null
                },
                null
            };
            var leafResultColumnIds = new uint[] { 0, 1, 3 };
            var createQueryOut = CreateQueryWithCategorization(searchScope, mappedProps, columnIdsForGrouping, rangePivots, leafResultColumnIds);
            hierarchicalCursors = createQueryOut.aCursors;

            var propsForBindings = new Dictionary<uint, CFullPropSpec[]>
            {
                [0] = new CFullPropSpec[] { mappedProps[columnIdsForGrouping[0]] },
                [1] = new CFullPropSpec[] { mappedProps[columnIdsForGrouping[1]] },
                [2] = new CFullPropSpec[] { mappedProps[columnIdsForGrouping[2]] },
                [3] = new CFullPropSpec[] { mappedProps[leafResultColumnIds[0]], mappedProps[leafResultColumnIds[1]], mappedProps[leafResultColumnIds[2]] }
            };
            SetBindingsForHierarchicalCursors(propsForBindings);

            var rowStrings = GetChapteredResultsFromRows(3);
            var expectedResults = new string[]
            {
                "AAA;",
                "    d;",
                "        43520;",
                "            AAA;[GGG];kkk1.ppt;",
                "    NULL;",
                "        4;",
                "            AAA;NULL;abc1.txt;",
                "        30;",
                "            AAA;NULL;cda1.txt;",
                "BBB;",
                "    r;",
                "        27648;",
                "            BBB;[SSS];mmm2.xls;",
                "    NULL;",
                "        8;",
                "            BBB;NULL;abc2.txt;",
                "        12;",
                "            BBB;NULL;rst2.txt;",
                "        30;",
                "            BBB;NULL;cda2.txt;",
                "CreateQuery_Categorization;",
                "    ;",
                "        23552;",
                "            CreateQuery_Categorization;[AAA,BBB];ttt0.doc;",
                "        27648;",
                "            CreateQuery_Categorization;[ABC,AAA];mmm0.xls;",
                "    m;",
                "        43520;",
                "            CreateQuery_Categorization;[NNN,BBB];kkk0.ppt;",
                "    NULL;",
                "        0;",
                "            CreateQuery_Categorization;NULL;abc0.txt;",
                "        12;",
                "            CreateQuery_Categorization;NULL;rst0.txt;",
                "        30;",
                "            CreateQuery_Categorization;NULL;cda0.txt;",
                "        NULL;",
                "            CreateQuery_Categorization;NULL;AAA;",
                "            CreateQuery_Categorization;NULL;BBB;",
                "            CreateQuery_Categorization;NULL;EEE;",
                "            CreateQuery_Categorization;NULL;SSS;",
                "EEE;",
                "    ;",
                "        23552;",
                "            EEE;[AAA,CCC];ttt3.doc;",
                "    NULL;",
                "        11;",
                "            EEE;NULL;abc3.txt;",
                "        12;",
                "            EEE;NULL;rst3.txt;",
                "SSS;",
                "    NULL;",
                "        117;",
                "            SSS;NULL;ggg4.txt;",
            };

            var succeed = expectedResults.SequenceEqual(rowStrings);
            Site.Assert.IsTrue(succeed, "The hierarchical chaptered results should be the same as the expected results.");
        }

        #endregion

        private List<string> GetChapteredResultsFromRows(uint chapterDepth)
        {
            // This variable is to record the current chapter value of each cursor for getting chaptered rows.
            var chapterMap = new Dictionary<uint, uint>();
            for (var currentDepth = 0U; currentDepth <= chapterDepth; currentDepth++)
            {
                chapterMap.Add(currentDepth, 1);
            }

            var rowStrings = new List<string>();
            GetChapteredRows(chapterDepth, 0, chapterMap, rowStrings);

            Site.Log.Add(LogEntryKind.Debug, "Start to print all rows obtained by the query.");
            foreach (var rowString in rowStrings)
            {
                Site.Log.Add(LogEntryKind.Debug, rowString);
            }
            Site.Log.Add(LogEntryKind.Debug, "All rows have been printed.");

            return rowStrings;
        }

        private void GetChapteredRows(uint chapterDepth, uint currentDepth, Dictionary<uint, uint> chapterMap, List<string> rowStrings)
        {
            if (currentDepth < chapterDepth)
            {
                if (currentDepth == 0)
                {
                    chapterMap[currentDepth] = 0;
                }

                wspAdapter.CPMGetRowsIn(
                    hierarchicalCursors[currentDepth],
                    1,
                    wspAdapter.builder.parameter.EachRowSize,
                    wspAdapter.builder.parameter.BufferSize,
                    0,
                    (uint)eType_Values.eRowSeekNext,
                    chapterMap[currentDepth],
                    new CRowSeekNext { _cskip = 0 },
                    out var getRowsOut);

                if (getRowsOut._cRowsReturned > 0)
                {
                    rowStrings.AddRange(GetPrintableRows(currentDepth, getRowsOut.Rows));

                    if (currentDepth + 1 > chapterDepth)
                    {
                        return;
                    }

                    GetChapteredRows(chapterDepth, currentDepth + 1, chapterMap, rowStrings);
                }
                else
                {
                    if (currentDepth == 0)
                    {
                        return;
                    }

                    chapterMap[currentDepth] = chapterMap[currentDepth] + 1;
                    GetChapteredRows(chapterDepth, currentDepth - 1, chapterMap, rowStrings);
                }
            }
            else
            {
                while (true)
                {
                    wspAdapter.CPMGetRowsIn(
                        hierarchicalCursors[currentDepth],
                        1,
                        wspAdapter.builder.parameter.EachRowSize,
                        wspAdapter.builder.parameter.BufferSize,
                        0,
                        (uint)eType_Values.eRowSeekNext,
                        chapterMap[currentDepth],
                        new CRowSeekNext { _cskip = 0 },
                        out var getRowsOut);

                    if (getRowsOut._cRowsReturned > 0)
                    {
                        rowStrings.AddRange(GetPrintableRows(currentDepth, getRowsOut.Rows));
                    }
                    else
                    {
                        if (currentDepth == 0)
                        {
                            return;
                        }

                        chapterMap[currentDepth] = chapterMap[currentDepth] + 1;
                        GetChapteredRows(chapterDepth, currentDepth - 1, chapterMap, rowStrings);
                        return;
                    }
                }
            }
        }

        private List<string> GetPrintableRows(uint chapterDepth, Row[] rows)
        {
            var ret = new List<string>();

            foreach (var row in rows)
            {
                var stringBuilder = new StringBuilder();
                stringBuilder.Append(' ', (int)(chapterDepth * 4));
                foreach (var column in row.Columns)
                {
                    if (column.Data != null)
                    {
                        if (column.Data is string[])
                        {
                            var stringVector = column.Data as string[];
                            stringBuilder.Append('[');
                            foreach (var element in stringVector)
                            {
                                stringBuilder.Append($"{element},");
                            }
                            stringBuilder.Remove(stringBuilder.Length - 1, 1);
                            stringBuilder.Append("];");
                        }
                        else
                        {
                            stringBuilder.Append($"{column.Data};");
                        }
                    }
                    else
                    {
                        stringBuilder.Append("NULL;");
                    }
                }

                ret.Add(stringBuilder.ToString());
            }

            return ret;
        }

        private CPMCreateQueryOut CreateQueryWithCategorization(
            string searchScope,
            CFullPropSpec[] mappedProps,
            uint[] columnIdsForGrouping,
            Dictionary<object, string>[] rangePivots,
            uint[] leafResultColumnIds)
        {
            argumentType = ArgumentType.AllValid;
            Site.Log.Add(LogEntryKind.TestStep, "Client sends CPMConnectIn and expects success.");
            wspAdapter.CPMConnectInRequest();

            var searchScopeRetriction = wspAdapter.builder.GetPropertyRestriction(
                    _relop_Values.PREQ,
                    WspConsts.System_Search_Scope,
                    wspAdapter.builder.GetBaseStorageVariant(vType_Values.VT_LPWSTR, new VT_LPWSTR(searchScope)));
            var restrictionArray = new CRestrictionArray { count = 1, isPresent = 1, Restriction = searchScopeRetriction };

            var columnSet = wspAdapter.builder.GetColumnSet(mappedProps.Length);

            var pidMapper = GetPidMapper(mappedProps);

            var sortSets = GetCInGroupSortAggregSets(columnIdsForGrouping, leafResultColumnIds);

            var specs = new CCategorizationSpec[columnIdsForGrouping.Length];
            for (var idx = 0U; idx < rangePivots.Length; idx++)
            {
                if (rangePivots[idx] != null)
                {
                    specs[idx] = GetRangeCCategorizationSpec(rangePivots[idx], columnIdsForGrouping[idx]);
                }
                else
                {
                    specs[idx] = GetUniqueCCategorizationSpec(columnIdsForGrouping[idx]);
                }
            }
            var categorizationSet = GetCCategorizationSet(specs);
            Site.Log.Add(LogEntryKind.TestStep, $"Client sends CPMCreateQueryIn and expects success.");
            wspAdapter.CPMCreateQueryIn(
                columnSet,
                restrictionArray,
                sortSets,
                categorizationSet,
                new CRowsetProperties(),
                pidMapper,
                new CColumnGroupArray(),
                wspAdapter.builder.parameter.LCID_VALUE,
                out var createQueryOut);

            return createQueryOut;
        }

        private void SetBindingsForHierarchicalCursors(Dictionary<uint, CFullPropSpec[]> propsForBindings)
        {
            for (var idx = 0U; idx < hierarchicalCursors.Length; idx++)
            {
                var columns = new List<CTableColumn>();
                foreach (var prop in propsForBindings[idx])
                {
                    columns.Add(wspAdapter.builder.GetTableColumn(prop, vType_Values.VT_VARIANT));
                }

                Site.Log.Add(LogEntryKind.TestStep, "Client sends CPMSetBindingsIn and expects success.");
                wspAdapter.CPMSetBindingsIn(
                    hierarchicalCursors[idx],
                    wspAdapter.builder.parameter.EachRowSize,
                    (uint)columns.Count,
                    columns.ToArray());
            }
        }

        private CPidMapper GetPidMapper(params CFullPropSpec[] mappedProps)
        {
            var ret = new CPidMapper();
            ret.aPropSpec = mappedProps;
            ret.count = (uint)ret.aPropSpec.Length;

            return ret;
        }

        private CInGroupSortAggregSets GetCInGroupSortAggregSets(uint[] columnIdsForGrouping, uint[] leafResultColumnIds)
        {
            var ret = new CInGroupSortAggregSets();
            ret.cCount = 1;
            ret.SortSets = new CSortSet[1];
            ret.SortSets[0].count = (uint)(columnIdsForGrouping.Length + leafResultColumnIds.Length);

            var sortArray = new CSort[ret.SortSets[0].count];
            var idx = 0;
            foreach (var columnId in columnIdsForGrouping.Concat(leafResultColumnIds))
            {
                sortArray[idx] = new CSort
                {
                    dwOrder = dwOrder_Values.QUERY_SORTASCEND,
                    dwIndividual = dwIndividual_Values.QUERY_SORTALL,
                    pidColumn = columnId,
                    locale = wspAdapter.builder.parameter.LCID_VALUE
                };
                idx++;
            }

            ret.SortSets[0].sortArray = sortArray;
            return ret;
        }

        private CCategorizationSet GetCCategorizationSet(params CCategorizationSpec[] categSpecs)
        {
            var ret = new CCategorizationSet();
            ret.count = (uint)categSpecs.Length;
            ret.categories = categSpecs;

            return ret;
        }

        private CCategorizationSpec GetUniqueCCategorizationSpec(uint columnIdForGrouping)
        {
            var lcid = wspAdapter.builder.parameter.LCID_VALUE;

            var ret = new CCategorizationSpec();

            var csColumn = new CColumnSet
            {
                count = 0,
                indexes = new uint[0]
            };
            ret._csColumns = csColumn;

            var spec = new CCategSpec();
            spec._ulCategType = _ulCategType_Values.CATEGORIZE_UNIQUE;
            var sortKey = new CSort
            {
                pidColumn = columnIdForGrouping,
                dwOrder = dwOrder_Values.QUERY_SORTASCEND,
                dwIndividual = dwIndividual_Values.QUERY_SORTALL,
                locale = lcid
            };
            spec._sortKey = sortKey;
            ret._Spec = spec;

            ret._AggregSet = new CAggregSet { cCount = 0, AggregSpecs = new CAggregSpec[0] };
            ret._SortAggregSet = new CSortAggregSet { cCount = 0, SortKeys = new CAggregSortKey[0] };
            ret._InGroupSortAggregSets = new CInGroupSortAggregSets { cCount = 0, Reserved = 0, SortSets = new CSortSet[0] };

            return ret;
        }

        private CCategorizationSpec GetRangeCCategorizationSpec(Dictionary<object, string> rangePivots, uint columnIdForGrouping)
        {
            var lcid = wspAdapter.builder.parameter.LCID_VALUE;

            vType_Values prValVType;
            var keyType = rangePivots.First().Key.GetType();
            if (keyType == typeof(ulong))
            {
                prValVType = vType_Values.VT_UI8;
            }
            else if (keyType == typeof(string))
            {
                prValVType = vType_Values.VT_LPWSTR;
            }
            else
            {
                throw new NotImplementedException($"The process for {keyType} is not implemented.");
            }

            var ret = new CCategorizationSpec();

            var csColumn = new CColumnSet
            {
                count = 0,
                indexes = new uint[0]
            };
            ret._csColumns = csColumn;

            var spec = new CCategSpec();
            spec._ulCategType = _ulCategType_Values.CATEGORIZE_RANGE;
            var sortKey = new CSort
            {
                pidColumn = columnIdForGrouping,
                dwOrder = dwOrder_Values.QUERY_SORTASCEND,
                dwIndividual = dwIndividual_Values.QUERY_SORTALL,
                locale = lcid
            };
            spec._sortKey = sortKey;
            var ranges = new CRangeCategSpec();
            ranges._lcid = lcid;
            ranges.cRange = (uint)rangePivots.Count;
            var boundaries = new RANGEBOUNDARY[ranges.cRange];
            var idx = 0;
            foreach (var kv in rangePivots)
            {
                var (pivot, label) = (kv.Key, kv.Value);
                boundaries[idx] = new RANGEBOUNDARY
                {
                    ulType = RANGEBOUNDARY_ulType_Values.DBRANGEBOUNDTTYPE_EXACT,
                    prVal = wspAdapter.builder.GetBaseStorageVariant(prValVType, GetValueByVType(prValVType, pivot)),
                };

                if (string.IsNullOrEmpty(label))
                {
                    boundaries[idx].labelPresent = 0x0;
                }
                else
                {
                    boundaries[idx].labelPresent = 0x1;
                    boundaries[idx].ccLabel = (uint)label.Length;
                    boundaries[idx].Label = label;
                }

                idx++;
            }
            ranges.aRangeBegin = boundaries;
            spec.CRangeCategSpec = ranges;
            ret._Spec = spec;

            ret._AggregSet = new CAggregSet { cCount = 0, AggregSpecs = new CAggregSpec[0] };
            ret._SortAggregSet = new CSortAggregSet { cCount = 0, SortKeys = new CAggregSortKey[0] };
            ret._InGroupSortAggregSets = new CInGroupSortAggregSets { cCount = 0, Reserved = 0, SortSets = new CSortSet[0] };

            return ret;
        }

        private object GetValueByVType(vType_Values prValVType, object pivot)
        {
            switch (prValVType)
            {
                case vType_Values.VT_UI8:
                    return pivot;

                case vType_Values.VT_LPWSTR:
                    return new VT_LPWSTR(pivot as string);

                default:
                    throw new NotImplementedException($"The process for {prValVType} is not implemented.");
            }
        }
    }
}