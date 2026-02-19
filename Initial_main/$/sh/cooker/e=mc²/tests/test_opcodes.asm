-(Test all required opcodes for task 3.1)

@main:
    -(Test load.const)
    :: load.const << 10 >> -> %r0
    :: load.const << 20 >> -> %r1
    
    -(Test add)
    :: add << %r0, %r1 >> -> %r2
    
    -(Test mul)
    :: mul << %r2, 2 >> -> %r3
    
    -(Test store and load)
    :: store.local << %r3 >> -> -8
    :: load.local << -8 >> -> %r4
    
    -(Test call)
    :: call << print_value >> << %r4 >>
    
    -(Test jump)
    :: jump << @end >>
    
@print_value:
    -(Test syscall)
    :: syscall << io_write >> << %r0 >>
    :: return
    
@end:
    :: return << %r4 >>
