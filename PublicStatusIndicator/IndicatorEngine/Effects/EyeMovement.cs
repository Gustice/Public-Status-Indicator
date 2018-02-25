namespace PublicStatusIndicator.IndicatorEngine
{
    /// <summary>
    /// Eye Movement.
    /// Provide method to move eye from fixepoint to fixepoint in a parabolic matter.
    /// </summary>
    public class EyeMovement
    {
        private bool _movementFinished = false;
        /// <summary>
        /// Displays wheter moving is finished
        /// Manipulating resets moving point
        /// </summary>
        public bool MovementFinished
        {
            get { return _movementFinished; }
            set {
                _movementFinished = value;
                _phi = 0;
            }
        }

        public int DemandedDelta = 0;
        public int Reference_Phi_0 = 0;

        //int _deltaDirection = 0;
        float _alpha = 0;

        int _AccStep = 0xFFFF;
        int _tempT = 0;

        float _phi = 0;
        float _phi_T_2 = 0;
        float _omega_T_2 = 0;

        int _phi_T = 0;
        int _halfDuration;


        /// <summary>
        /// Initiate to approach new fixpoint
        /// </summary>
        /// <param name="delta">if not 0, a new fixpoint will be approached relatively to current position</param>
        /// <param name="duration">if not 0, the default movement time is overwritten with given value, works only together wit new fixpoint position</param>
        public virtual void InitNewMove(int delta, int duration = 0)
        {
            DemandedDelta = delta;
            if (duration != 0)
            {
                _halfDuration = duration / 2;
                _alpha = (float)delta / 2 * 2 / (_halfDuration * _halfDuration);
            }
            _AccStep = 0;
            _tempT = 0;
            Reference_Phi_0 = _phi_T;
            MovementFinished = false;
        }

        /// <summary>
        /// Execute on moveing step. Movement followes the following equasions.
        /// phi(t) = alpha / 2 * t^2
        /// Where alpha = alpha_min = Phi/2 * 8 / T^2, which is the minimum acceleration to finish movement "Phi" in demanded period "T"
        /// </summary>
        /// <returns></returns>
        public int MovingStep()
        {
            _tempT++;
            switch (_AccStep)
            {
                // initiative acceleration
                case 0:
                    _phi = _alpha * (_tempT * _tempT) / 2;
                    _AccStep++;
                    break;

                // acceleration
                case 1:
                    _phi = _alpha * (_tempT * _tempT) / 2;

                    if (_tempT >= _halfDuration) // At half way through
                    {
                        _omega_T_2 = _alpha * _tempT;   // save gained velocity
                        _phi_T_2 = _phi;                // save gained Phi
                        _tempT = 0;
                        _AccStep++;
                    }
                    break;

                // deceleration with gained velocity relative to gained Phi
                case 2:
                    _phi = _phi_T_2 + _omega_T_2 * _tempT - _alpha * (_tempT * _tempT) / 2;
                    if (_tempT >= _halfDuration)
                    {
                        _phi_T = (int)_phi;
                        _AccStep++;

                        _movementFinished = true;
                    }
                    break;

                default:
                    // Make just nothing
                    break;
            }

            return (int)_phi;
        }

        /// <summary>
        /// Return displacement relatively to last position
        /// </summary>
        /// <returns></returns>
        public int MemorizedMovingStep()
        {
            return Reference_Phi_0 + MovingStep();
        }
    }
}