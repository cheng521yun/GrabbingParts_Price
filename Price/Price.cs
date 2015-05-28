using GrabbingParts.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrabbingParts.BLL.Price
{
    public class Price
    {
        public List<List<PriceResult>> GetPrice(string mid)
        {
            List<IPrice> al = new List<IPrice>();

            List<List<PriceResult>> resultList = new List<List<PriceResult>>();
            
            IPrice digikey = new DigiKey();
            al.Add(digikey);

            IPrice icbase = new ICBase();
            al.Add(icbase);

            IPrice rocelec = new Rocelec();
            al.Add(rocelec);

            IPrice onlinecom = new Onlinecomponents();
            al.Add(onlinecom);

            IPrice alliedelec = new Alliedelec();
            al.Add(alliedelec);

            IPrice microchipdirect = new Microchipdirect();
            al.Add(microchipdirect);

            IPrice findchips = new Findchips();
            al.Add(findchips);

            List<System.Threading.Tasks.Task<List<PriceResult>>> tasks = new List<System.Threading.Tasks.Task<List<PriceResult>>>();
            foreach(IPrice iprice in al)
            {
                tasks.Add(Task<List<PriceResult>>.Factory.StartNew(() => iprice.GetPrice(mid)));
            }
            Task.WaitAll(tasks.ToArray());

            for (int i = 0; i < tasks.Count();i++ )
            {
                resultList.Add(tasks[i].Result);
            }

            return resultList;
        }
    }
}
