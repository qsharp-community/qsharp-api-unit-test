namespace Microsoft.Quantum.Demo {
    
    /// # Summary
    /// Entry point for the demo. 
    ///
    /// # Example
    /// ```qsharp
    /// function TestIncrementAny () : Unit {
    ///     let output = Increment(1);
    ///     EqualityFactI(2, output, "wrong return value");
    /// }
    /// ```
    /// Another example:
    /// ```qsharp
    /// function TestIncrement2 () : Unit {
    ///     let output = Increment(2);
    ///     EqualityFactI(3, output, "wrong return value");
    /// }
    /// ```
    @EntryPoint()
    function Increment(arg : Int) : Int {
        return arg + 1;
    }
}

namespace Microsoft.Quantum.Demo2 {
    
    /// # Summary
    /// Entry point for the demo. 
    ///
    /// # Example
    /// ```qsharp
    /// function TestIncrement1 () : Unit {
    ///     let output = Increment(1);
    ///     EqualityFactI(2, output, "wrong return value");
    /// }
    /// ```
    /// Another example:
    /// ```qsharp
    /// function TestIncrement2 () : Unit {
    ///     let output = Increment(2);
    ///     EqualityFactI(3, output, "wrong return value");
    /// }
    /// ```
    function Increment(arg : Int) : Int {
        return arg + 1;
    }
}
