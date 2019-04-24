using System;

internal interface IMockInterfaceObject
{
    Guid ObjectId { get; set; }
    string Name { get; }
}

internal interface IMockWithDependency
{
    IMockInterfaceObject MockInterfaceObject { get; set; }
}

internal class MockObject : IMockInterfaceObject
{
    public Guid ObjectId { get; set; }

    public string Name { get; }

    public MockObject()
    {
        this.ObjectId = Guid.NewGuid();
        this.Name = nameof(MockObject) + DateTime.Now;
    }
}

internal class MockWithDependency : IMockWithDependency
{
    public IMockInterfaceObject MockInterfaceObject { get; set; }

    public MockWithDependency(IMockInterfaceObject mockObject)
    {
        this.MockInterfaceObject = mockObject;
    }
}
