using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BANWlLib.mainUI.pojo
{
    public class shot
    {
        // 货币 defname
        public string CurrencyDefName { get; set; }

        // 货币数量
        public int CurrencyAmount { get; set; }

        // 货币图片 URL/路径
        public string CurrencyImage { get; set; }

        // 商品数量
        public int ProductAmount { get; set; }

        // 商品图片 URL/路径
        public string ProductImage { get; set; }

        // 商品 defname
        public string ProductDefName { get; set; }

        // 商品名称
        public string ProductName { get; set; }

        // 商品介绍
        public string ProductDescription { get; set; }

        // 商品品质（文字：普通/稀有/史诗等）
        public string ProductQuality { get; set; }

        public shot() { }

        public shot(string currencyDefName, int currencyAmount, string currencyImage,
                        int productAmount, string productImage, string productDefName,
                        string productName, string productDescription, string productQuality)
        {
            CurrencyDefName = currencyDefName;
            CurrencyAmount = currencyAmount;
            CurrencyImage = currencyImage;
            ProductAmount = productAmount;
            ProductImage = productImage;
            ProductDefName = productDefName;
            ProductName = productName;
            ProductDescription = productDescription;
            ProductQuality = productQuality;
        }
    }
}
