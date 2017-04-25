This repository contains experimental codes.

# EasyCompositeIterator

This library provides access to data structures which is designed by composite pattern.  
This codes emphasizes code readability rather than performance.  
Composite pattern: <https://en.wikipedia.org/wiki/Composite_pattern>

## Usage
### Supported composite design
- The component class must have a get method of the element name.
- The component class must have a get method of the value.
- The component class must have a get method of the child elements.  

If the class has these methods, you can use this library to access the data structure.

### 0. Create the iterator.
CompositeIterator.Create\<T\> provides a new instance.
```
var iterator = CompositeIterator<Component>.Create(
    new List<Component> { component1, component2, component3, ... },
    c => c.ElementName, // to access the element name
    c => c.Value,       // to access the value
    c => c.Children);   // to access the children
```
or use a single component
```
var iterator = CompositeIterator<Component>.Create(
    rootComponent,
    c => c.ElementName, // to access the element name
    c => c.Value,       // to access the value
    c => c.Children);   // to access the children
```

### 1. Get a leaf value (using dynamic)
Get a dynamic object by AsDynamic() method and access the value as property.  
This method can access the same level leaf only.  
Example)
```
var d = iterator.AsDynamic();
var x = d.XXX; // Get the value of the leaf name "XXX"
```

### 2. Get a leaf value (using GetValue method)
Get a value without dynamic object.  
This method can access the same level leaf only.  
Example)
```
var d = iterator.GetValue<int>("XXX");
```

### 3. Get leaf values of same element name
If the same name leafs exist, use GetValues method.  
Example)
```
ROOT
  - AAA = 1
  - AAA = 2
  - BBB = 3
```
```
var d = iterator.GetValues<int>("AAA");
d -> int[] { 1, 2 }
```

### 4. Access a composite
SingleChild() iterates the composite of the specific name and returns a specific type instance.  
Example)  
This example creates a point instance (x, y) = (ROOT->XXX, ROOT->YYY).
```
ROOT
  - PT
    - XXX = 10
    - YYY = 20
```
```
var point = iterator.SingleChild(
    "PT",
    pt =>
    {
        var d = pt.AsDynamic();
        var x = d.XXX; // or d.GetValue<int>("XXX")
        var y = d.YYY; // or d.GetValue<int>("YYY")
        return new Point(x, y);
    });
point -> (x = 10, y = 20)
```

### 5. Iterate multiple composites
MultipleChild() iterates the composites of the specific name.  
Example)  
```
ROOT
  - PointData
    - XXX = 1
    - YYY = 2
  - PointData
    - XXX = 3
    - YYY = 4
  - PointData
    - XXX = 5
    - YYY = 6
```
```
var pointList = iterator.MultiChild(
    "PointData",
    pt =>
    {
        var d = pt.AsDynamic();
        var x = d.XXX; // or d.GetValue<int>("XXX")
        var y = d.YYY; // or d.GetValue<int>("YYY")
        return new Point(x, y);
    }).ToList();
pointList -> List<Point> { Point(1, 2), Point(3, 4), Point(5,6) }
```

### 6. Extract element
Example)
```
Component root = iterator.Extract("ROOT");
```
