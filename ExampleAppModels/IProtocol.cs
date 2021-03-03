using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExampleAppModels
{
    public interface IProtocol
    {
        void CSOp1(int x, int y);

        void CSOpt2(List<TestClasss> list);

        Task<int> CSOp3(int z);

        int CSOp4(int z);

        int CSSomeExceptionMethod();
    }
}
