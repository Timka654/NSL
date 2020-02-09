using BinarySerializer;
using BinarySerializer.DefaultTypes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace BinarySerializer_v5.Test
{
    public class DebugTemp
    {

        public class RecurciveError
        {
            [Binary(typeof(BinaryInt32))]
            [BinaryScheme("default")]
            public int q { get; set; } = 12;


            [Binary(typeof(BinaryList16<tempStruct>))]
            [BinaryScheme("default")]
            public List<tempStruct> t { get; set; }
        }

        public class tempStruct
        {

            [Binary(typeof(BinaryInt32))]
            [BinaryScheme("default")]
            public int q { get; set; } = 22;
        }

        public class ResourceInfo
        {
            [Binary(typeof(BinaryInt64))]
            [BinaryScheme("PrivateInfo")]
            [BinaryScheme("RewardResource")]
            [BinaryScheme("default")]
            public long ResourceSilver { get; set; }

            [Binary(typeof(BinaryInt64))]
            [BinaryScheme("PrivateInfo")]
            [BinaryScheme("RewardResource")]
            [BinaryScheme("default")]
            public long ResourceGold { get; set; }

            /// <summary>
            /// Железо
            /// </summary>
            [Binary(typeof(BinaryInt32))]
            [BinaryScheme("PrivateInfo")]
            [BinaryScheme("RewardResource")]
            [BinaryScheme("default")]
            public int ResourceIron { get; set; }

            /// <summary>
            /// Камень
            /// </summary>
            [Binary(typeof(BinaryInt32))]
            [BinaryScheme("PrivateInfo")]
            [BinaryScheme("RewardResource")]
            [BinaryScheme("default")]
            public int ResourceStone { get; set; }

            /// <summary>
            /// Дерево
            /// </summary>
            [Binary(typeof(BinaryInt32))]
            [BinaryScheme("PrivateInfo")]
            [BinaryScheme("RewardResource")]
            [BinaryScheme("default")]
            public int ResourceWood { get; set; }

            /// <summary>
            /// Медь
            /// </summary>
            [Binary(typeof(BinaryInt32))]
            [BinaryScheme("PrivateInfo")]
            [BinaryScheme("RewardResource")]
            [BinaryScheme("default")]
            public int ResourceBrass { get; set; }

            /// <summary>
            /// Бронза
            /// </summary>
            [Binary(typeof(BinaryInt32))]
            [BinaryScheme("PrivateInfo")]
            [BinaryScheme("RewardResource")]
            [BinaryScheme("default")]
            public int ResourceBronze { get; set; }

            /// <summary>
            /// Глина
            /// </summary>
            [Binary(typeof(BinaryInt32))]
            [BinaryScheme("PrivateInfo")]
            [BinaryScheme("RewardResource")]
            [BinaryScheme("default")]
            public int ResourceClay { get; set; }

            public static ResourceInfo operator +(ResourceInfo info1, ResourceInfo info2)
            {
                ResourceInfo r = new ResourceInfo();
                r.ResourceSilver = info1.ResourceSilver + info2.ResourceSilver;
                r.ResourceGold = info1.ResourceGold + info2.ResourceGold;
                r.ResourceBrass = info1.ResourceBrass + info2.ResourceBrass;
                r.ResourceBronze = info1.ResourceBronze + info2.ResourceBronze;
                r.ResourceClay = info1.ResourceClay + info2.ResourceClay;
                r.ResourceIron = info1.ResourceIron + info2.ResourceIron;
                r.ResourceStone = info1.ResourceStone + info2.ResourceStone;
                r.ResourceWood = info1.ResourceWood + info2.ResourceWood;

                return r;
            }
            public static ResourceInfo operator -(ResourceInfo info1, ResourceInfo info2)
            {
                ResourceInfo r = new ResourceInfo();
                r.ResourceSilver = info1.ResourceSilver - info2.ResourceSilver;
                r.ResourceGold = info1.ResourceGold - info2.ResourceGold;
                r.ResourceBrass = info1.ResourceBrass - info2.ResourceBrass;
                r.ResourceBronze = info1.ResourceBronze - info2.ResourceBronze;
                r.ResourceClay = info1.ResourceClay - info2.ResourceClay;
                r.ResourceIron = info1.ResourceIron - info2.ResourceIron;
                r.ResourceStone = info1.ResourceStone - info2.ResourceStone;
                r.ResourceWood = info1.ResourceWood - info2.ResourceWood;

                return r;
            }
        }

        public class StringTest
        {
            [Binary(typeof(BinaryInt32))]
            [BinaryScheme("Search")]
            [BinaryScheme("PrivateInfo")]
            public int Id { get; set; }

            [Binary(typeof(BinaryString16))]
            [BinaryScheme("Search")]
            [BinaryScheme("PrivateInfo")]
            public string Name { get; set; }

            [Binary(typeof(BinaryString16))]
            [BinaryScheme("PrivateInfo")]
            public string Notice { get; set; }

            [Binary(typeof(BinaryBool))]
            [BinaryScheme("Search")]
            [BinaryScheme("PrivateInfo")]
            [BinaryScheme("UpdateSettings")]
            public bool AutoAccept { get; set; }

            [Binary(typeof(BinaryNullable<BinaryBool,bool>))]
            [BinaryScheme("Search")]
            [BinaryScheme("PrivateInfo")]
            [BinaryScheme("UpdateSettings")]
            public bool? AutoAccept2 { get; set; }

            [Binary(typeof(ResourceInfo))]
            [BinaryScheme("PrivateInfo")]
            public ResourceInfo ResourceInfo { get; set; }

            protected int memberCount;
        }

        public class TStruct
        {
            public static void Debug()
            {
                BinarySerializer.BinarySerializer bs = new BinarySerializer.BinarySerializer();

                StringTest st = new StringTest();

                st.Name = "Guild top 1";
                st.Notice = "";
                st.ResourceInfo = new ResourceInfo();
                st.AutoAccept2 = true;

                var bf = bs.Serialize("PrivateInfo", st);

                var ds = bs.Deserialize<StringTest>("PrivateInfo", bf);

                RecurciveError r = new RecurciveError();
                r.t = new List<tempStruct>();
                for (int i = 0; i < 5; i++)
                {
                    r.t.Add(new tempStruct());
                }



                var buf3 = bs.Serialize("default", r);

            }
        }
    }
}
