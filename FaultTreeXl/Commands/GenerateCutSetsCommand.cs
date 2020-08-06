using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FaultTreeXl
{
    internal class GenerateCutSetsCommand : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        class ANDSorter : IComparer<AND>
        {
            public int Compare(AND x, AND y)
            {
                return -x.PFD.CompareTo(y.PFD);
            }
        }

        public void Execute(object parameter)
        {
            if (parameter is FaultTreeModel ftm)
            {
                if (ftm.ShowingCutsets)
                {
                    ftm.RootNode = ftm.SavedRootNode;
                    ftm.ReDrawRootNode();
                    ftm.ShowingCutsets = false;
                }
                else
                {
                    List<CutSet> theCutSets = ftm.RootNode.CutSets;

                    var newRootNode = new OR() { Name = "CS0", Description = "Cut Sets" };
                    var i = 1;
                    var sortList = new List<AND>();
                    foreach (CutSet cs in theCutSets)
                    {
                        AND newAND = new AND() { Name = $"CS: {i++}" };
                        foreach (GraphicItem n in cs)
                        {
                            newAND.Nodes.Add(new Node() { Name = n.Name, Description = n.Description, Lambda = n.Lambda, PTI = n.PTI });
                        }
                        sortList.Add(newAND);
                    }
                    sortList.Sort(new ANDSorter());

                    foreach (var sortedNode in sortList)
                    {
                        newRootNode.Nodes.Add(sortedNode);
                    }

                    ftm.SavedRootNode = ftm.RootNode;
                    ftm.RootNode = newRootNode;
                    ftm.ReDrawRootNode();
                    ftm.ShowingCutsets = true;
                }

                //IEnumerable<IEnumerable<GraphicItem>> x2 = x.CombinationsOf2().Distinct();

                //string list = string.Join("; ", x.Select(cs => "(" + string.Join(", ", cs.Select(n => n.Name)) + ")"));

                //string list2 = string.Join("; ", x2.Select(cs => "(" + string.Join(", ", cs.Select(n => n.Name)) + ")"));

                //IEnumerable<decimal> v3 = (from n1 in x
                //                           let y = (from n2 in n1
                //                                    select n2.Lambda * n2.PTI).Aggregate((decimal)1, (a, b) => a * b) / (n1.Count() + 1)
                //                           select y);

                //IEnumerable<decimal> v4 = (from n1 in x2
                //                           let y = (from n2 in n1
                //                                    select n2.Lambda * n2.PTI).Aggregate((decimal)1, (a, b) => a * b) / (n1.Count() + 1)
                //                           select y);

                //decimal v5 = v3.Sum();
                //decimal v6 = v4.Sum();
                //decimal v7 = v5 - v6;

                ////                var result = $"Intial list: {list}\nPair List: {list2}\nPFD: {v7}\nOverestimate: {v6}";
                //string result = $"Cut Sets: {list}";

                //CutSetList newResult = ftm.RootNode.CutSets2;
                ////string newText = string.Join("; ", newResult.Select(cs => "(" + string.Join(", ", cs.Select(n => n.Name)) + ")"));

                //System.Windows.MessageBox.Show(result);
                ////System.Windows.MessageBox.Show(newText);
            }
        }
    }

    static public class Utils
    {
        //public static IEnumerable<TSource> Prepend<TSource>(this IEnumerable<TSource> source, TSource item)
        //{
        //    if (source == null)
        //    {
        //        throw new ArgumentNullException("source");
        //    }

        //    yield return item;

        //    foreach (TSource element in source)
        //    {
        //        yield return element;
        //    }
        //}

        //public static IEnumerable<IEnumerable<TSource>> Permutate<TSource>(this IEnumerable<TSource> source)
        //{
        //    if (source == null)
        //    {
        //        throw new ArgumentNullException("source");
        //    }

        //    List<TSource> list = source.ToList();

        //    if (list.Count > 1)
        //    {
        //        return from s in list
        //               from p in Permutate(list.Take(list.IndexOf(s)).Concat(list.Skip(list.IndexOf(s) + 1)))
        //               select p.Prepend(s);
        //    }

        //    return new[] { list };
        //}

        //public static IEnumerable<IEnumerable<TSource>> Combinate<TSource>(this IEnumerable<TSource> source, int k)
        //{
        //    if (source == null)
        //    {
        //        throw new ArgumentNullException("source");
        //    }

        //    List<TSource> list = source.ToList();
        //    if (k > list.Count)
        //    {
        //        throw new ArgumentOutOfRangeException("k");
        //    }

        //    if (k == 0)
        //    {
        //        yield return Enumerable.Empty<TSource>();
        //    }

        //    foreach(var l in list)
        //    {
        //        foreach (var c in Combinate(list.Skip(1), k - 1))
        //        {
        //            yield return c.Prepend(l);
        //        }
        //    }


        //    //foreach (TSource l in list)
        //    //{
        //    //    var rest = list.Select(n => n).ToList();
        //    //    rest.Remove(l);

        //    //    var l3 = Combinate(rest, k - 1);
        //    //    foreach (var c in l3)
        //    //    {
        //    //        yield return c.Prepend(l);
        //    //    }
        //    //}
        //}

        public static IEnumerable<IEnumerable<T>> CombinationsOf2<T>(this IEnumerable<IEnumerable<T>> source)
        {
            IEnumerable<T>[] array1 = source.ToArray();
            for (int i = 0; i < array1.Length - 1; i++)
            {
                for (int j = i + 1; j < array1.Length; j++)
                {
                    yield return array1[i].Concat(array1[j]).Distinct();
                }
            }
        }

        //public static IEnumerable<IEnumerable<T>> CombinationsOfK<T>(this IEnumerable<T> data, int k)
        //{
        //    int size = data.Count();

        //    IEnumerable<IEnumerable<T>> Runner(IEnumerable<T> list, int n)
        //    {
        //        int skip = 1;
        //        foreach (var headList in list.Take(size - k + 1).Select(h => new T[] { h }))
        //        {
        //            if (n == 1)
        //                yield return headList.Distinct();
        //            else
        //            {
        //                foreach (var tailList in Runner(list.Skip(skip), n - 1))
        //                {
        //                    yield return headList.Concat(tailList).Distinct();
        //                }
        //                skip++;
        //            }
        //        }
        //    }

        //    return Runner(data.Distinct(), k);
        //}

    }
}
