using EnigmaSimulator.Domain;

EnigmaMachine machine = new(
    new Plugboard(),
    new Rotor(RotorSets.Enigma3, position: '1'),
    new Rotor(RotorSets.Enigma2, position: '1'),
    new Rotor(RotorSets.Enigma1, position: '1'),
    new Reflector(ReflectorSets.ReflectorB)
);