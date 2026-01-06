// HelloCpp.cpp
#include <iostream>

extern "C" __declspec(dllexport)
const char* SayHelloCpp() {
    return "Hello from C++";
}