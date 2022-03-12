using Test1;


var a = new A();
a.SetDeepValue(x => x.B.C, "FDDF");
a.SetDeepValue(x => x.B, new B());
Console.WriteLine(a.B.C);


class A
{
    private B b = new B();

    public B B
    {
        get { return b; }
        private set
        {
            b = value;
            Console.WriteLine("B set");
        }
    }
}

class B
{
    public string C { get; private set; } = "DD";
}