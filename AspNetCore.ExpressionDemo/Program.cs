using System;

namespace AspNetCore.ExpressionDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("欢迎大家来到朝夕教育架构班体验课！我是richard老师！");
                {
                    Console.WriteLine("****************认识表达式目录树*************");
                    //ExpressionTest.Show();
                }
                {
                    //Console.WriteLine("********************MapperTest********************");
                    //ExpressionTest.MapperTest();
                }
                {
                    //Console.WriteLine("********************解析表达式目录树********************");
                    ExpressionVisitorTest.Show();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.Read();
        }
    }
}
