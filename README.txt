1: Need to change Session.Sport from Cycling(2) to Running(1) (as below)
2: Might need to change any other messages that might contain a sport identifier, e.g.
	Lap,
	Sport settings (defines sports on devices)
	Goal
	Course
	Workout
	Totals
	Bike Profile
3: Also sport_bits which is a bit-encoding of the sport, and sport_index which I can't find mentioned anywhere

   public enum Sport : byte
   {
      Generic = 0, 
      Running = 1, 
      Cycling = 2, 
      Transition = 3, 
      FitnessEquipment = 4, 
      Swimming = 5, 
      Basketball = 6, 
      Soccer = 7, 
      Tennis = 8, 
      AmericanFootball = 9, 
      Training = 10, 
      Walking = 11, 
      CrossCountrySkiing = 12, 
      AlpineSkiing = 13, 
      Snowboarding = 14, 
      Rowing = 15, 
      Mountaineering = 16, 
      Hiking = 17, 
      Multisport = 18, 
      Paddling = 19, 
      Flying = 20, 
      EBiking = 21, 
      All = 254, 
      Invalid = 0xFF   
      
   }
   public enum SubSport : byte
   {
      Generic = 0, 
      Treadmill = 1, 
      Street = 2, 
      Trail = 3, 
      Track = 4, 
      Spin = 5, 
      IndoorCycling = 6, 
      Road = 7, 
      Mountain = 8, 
      Downhill = 9, 
      Recumbent = 10, 
      Cyclocross = 11, 
      HandCycling = 12, 
      TrackCycling = 13, 
      IndoorRowing = 14, 
      Elliptical = 15, 
      StairClimbing = 16, 
      LapSwimming = 17, 
      OpenWater = 18, 
      FlexibilityTraining = 19, 
      StrengthTraining = 20, 
      WarmUp = 21, 
      Match = 22, 
      Exercise = 23, 
      Challenge = 24, 
      IndoorSkiing = 25, 
      CardioTraining = 26, 
      IndoorWalking = 27, 
      EBikeFitness = 28, 
      All = 254, 
      Invalid = 0xFF   