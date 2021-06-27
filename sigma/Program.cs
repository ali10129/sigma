using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sigma
{
    class Program
    {
         class FlexPDE
        {
            public static int number_of_multipliers = 32;
            public int[] ooo;
            public int[] up;
            public int[,] down;
            public int[,] table;
            public int tableIndex;
            public FlexPDE()
            {
                ooo = new int[number_of_multipliers];
                up = new int[number_of_multipliers];
                down = new int[number_of_multipliers,2];
                table = new int[number_of_multipliers, 3];
                for (int i = 0; i < number_of_multipliers; i++)
                {
                    ooo[i] = 0;
                    up[i] = 0;
                    table[i, 0] = table[i, 1] = table[i, 2] = 0;
                }
                tableIndex = 0;
            }
            public void assignUp(int[] tmp)
            {
                for (int i = 0; i < tmp.Length; i++)
                {
                    up[i] = tmp[i];
                }
            }
            public void assignDown(int[,] streaming,int[,] streamingIndexer,int streamColumn,int k)
            {
                int[] tmp = new int[number_of_multipliers];
                int idx = 0;
                for (int i = 0; i < tmp.Length; i++)
                {
                    tmp[i] = 0;
                }
                for (int i = 0; i < tableIndex; i++)
                {
                    for (int j = 0; j < k ; j++)
                    {
                        if (this.table[i,1] == streamingIndexer[j,streamColumn])
                        {
                            down[this.table[i, 2],0] = streaming[j, streamColumn];        //????
                            down[this.table[i, 2], 1] = this.table[i, 0];        //????
                        }
                    }
                }
                calc();
            }
            public int[] calc()
            {
                for (int i = 0; i < number_of_multipliers; i++)
                {
                    ooo[i] = up[i] * down[i,0];
                }
                return ooo;
            }
            public static int[] merge(int m,FlexPDE[] PEs)
            {
                /* implemented without FAN */
                int[] column = new int[m];
                foreach (var item in PEs)
                {
                    for (int i = 0; i < FlexPDE.number_of_multipliers; i++)
                    {
                        column[item.down[i, 1]] += item.ooo[i];
                    }
                }

                return column;
            }
            public void ClearTable()
            {
                for (int i = 0; i < number_of_multipliers; i++)
                {
                    table[i, 0] = table[i, 1] = table[i, 2] = 0;
                    ooo[i] = 0;
                    down[i, 0] = down[i, 1] = 0;
                }
                tableIndex = 0;
            }
        }

        static void Main(string[] args)
        {
            Random _random = new Random();
            int m, n, k;
            m = 160;
            n = 150;
            k = 320;
            FlexPDE.number_of_multipliers = 32;


            int[,] streaming = new int[k, n];
            int[,] stationary = new int[m,k];

            int[,] o1 = new int[m, n];
            int[,] o2 = new int[m, n];

            bool[,] _streaming = new bool[k, n];
            bool[,] _stationary = new bool[m, k];


            //step 1
            int tmp = 0;
            for (int i = 0; i<k; i++)
            {
                for(int j = 0; j<n; j++)
                {
                    tmp = _random.Next(10);
                    streaming[i, j] = tmp < 5  ? tmp : 0;
                    if (streaming[i, j] > 0)
                        _streaming[i, j] = true;
                    else
                        _streaming[i, j] = false;
                }
            }
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < k; j++)
                {
                    tmp = _random.Next(10);
                    stationary[i, j] = tmp < 5 ? tmp : 0;
                    if (stationary[i, j] > 0)
                        _stationary[i, j] = true;
                    else
                        _stationary[i, j] = false;
                }
            }

            Console.WriteLine(); Console.WriteLine(); Console.WriteLine();
            Console.WriteLine("Streaming:");
            for (int i = 0; i < k; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    Console.Write(streaming[i, j] + ",");
                }
                Console.WriteLine();
            }
            Console.WriteLine(); Console.WriteLine(); Console.WriteLine();
            Console.WriteLine("Stationary:");
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < k; j++)
                {
                    Console.Write(stationary[i, j] + ",");
                }
                Console.WriteLine();
            }
            Console.WriteLine(); Console.WriteLine(); Console.WriteLine();


            //step 2
            bool[] temp1 = new bool[k];
            for (int i = 0; i < k; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    temp1[i] |= _streaming[i, j];
                }
                Console.WriteLine(temp1[i]);
            }
            Console.WriteLine("\n\n\n\n\n\bits");
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < k; j++)
                {

                    _stationary[i, j] &= temp1[j];
                    Console.Write(_stationary[i, j]);

                }
                Console.WriteLine();

            }
            Console.WriteLine("Stationary bitmap:");
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < k; j++)
                {
                    Console.Write(_stationary[i, j] + ",");
                }
                Console.WriteLine();
            }
            Console.WriteLine(); Console.WriteLine(); Console.WriteLine();

            //step 3
            int NumFlexPDE = 0;
            int numMulinFlexPDE = FlexPDE.number_of_multipliers;

            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < k; j++)
                {
                    if(_stationary[i, j])
                    {
                        NumFlexPDE++;
                    }
                }
            }
            Console.WriteLine("Number of Stationary Elements used in ...: " + NumFlexPDE);

            NumFlexPDE = NumFlexPDE % numMulinFlexPDE == 0 ? NumFlexPDE / numMulinFlexPDE : NumFlexPDE / numMulinFlexPDE + 1;
            Console.WriteLine("number of flexPDEs = " + NumFlexPDE);

            //step 4
            FlexPDE[] PEs = new FlexPDE[NumFlexPDE];
            for (int i = 0; i < NumFlexPDE; i++)
            {
                PEs[i] = new FlexPDE();
            }

            int counter = 0;
            int pdeID = 0;
            int[] temp2 = new int[numMulinFlexPDE];


            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < k; j++)
                {
                    if (_stationary[i,j])
                    {
                        temp2[counter++] = stationary[i, j];
                    }
                    if (counter == FlexPDE.number_of_multipliers)
                    {
                        PEs[pdeID++].assignUp(temp2);
                        counter = 0;
                        for (int jj = 0; jj < temp2.Length; jj++)
                        {
                            temp2[jj] = 0;
                        }

                    }
                }
            }
            if (counter > 0)
            {
                PEs[pdeID].assignUp(temp2);
            }
            //step 5 , 6 , 7
            int[,] streamingIndexer = new int[k, n];
            int[,] stationaryIndexer = new int[m, k];
            counter = 0;
            for (int i = 0; i < m; i++)
            {
                for(int j = 0; j < k; j++)
                {
                    stationaryIndexer[i, j] = -1;
                    if (_stationary[i,j])
                    {
                        stationaryIndexer[i, j] = counter++;
                        if (counter == FlexPDE.number_of_multipliers)
                        {
                            counter = 0;
                        }
                    }
                }
            }

            for (int i = 0; i < n; i++) //column index
            {
                counter = 0;
                for (int j = 0; j < k; j++) //row index
                {
                    streamingIndexer[j, i] = -1;
                    if (_streaming[j,i])
                    {
                        streamingIndexer[j, i] = counter++;
                    }
                }
            }

            for (int streamColumn = 0; streamColumn < n; streamColumn++)
            {

                pdeID = 0;
                counter = 0;

                for (int i = 0; i < m; i++)
                {
                    for (int j = 0; j < k; j++)
                    {
                        if (_stationary[i, j] == true && _streaming[j, streamColumn] == true)
                        {
                            PEs[counter].table[PEs[counter].tableIndex, 0] = i;     //row ID
                            PEs[counter].table[PEs[counter].tableIndex, 1] = streamingIndexer[j, streamColumn]; //source
                            PEs[counter].table[PEs[counter].tableIndex, 2] = stationaryIndexer[i, j]; //destination
                            PEs[counter].tableIndex++;
                        }

                        if (stationaryIndexer[i, j] == FlexPDE.number_of_multipliers - 1)
                        {
                            PEs[counter].assignDown(streaming,streamingIndexer,streamColumn,k);
                            counter++;
                        }
                    }
                }
                if (counter == PEs.Length - 1)
                {
                    PEs[counter].assignDown(streaming, streamingIndexer, streamColumn, k);
                }

                ///////////
                int[] o0;
                o0 = FlexPDE.merge(m, PEs);
                for (int ii = 0; ii < m; ii++)
                {
                    o1[ii, streamColumn] = o0[ii];
                }


                ///////////
                foreach (var item in PEs)
                {
                    item.ClearTable();
                }
            }
            Console.WriteLine("\n\n\n sigma output:");
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    Console.Write(o1[i, j]+ ",");
                }
                Console.WriteLine();
           }

            
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    for (int l = 0; l < k; l++)
                    {
                        o2[i, j] += stationary[i, l] * streaming[l, j];
                    }
                }
            }

            Console.WriteLine("\n\n\n computer output:");
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    Console.Write(o2[i, j] + ",");
                    if (o2[i, j] != o1[i, j])
                    {
                        Console.WriteLine("Errrrrrrrrrrrorrrrrrrr");
                    }
                }
                Console.WriteLine();
            }

            Console.ReadKey();
        }
    }
}
