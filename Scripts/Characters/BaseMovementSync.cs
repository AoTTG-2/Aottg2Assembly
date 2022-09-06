




using Photon;
using System;
using UnityEngine;

namespace Characters
{
    public class BaseMovementSync : Photon.MonoBehaviour
    {
        public bool Disabled;
        protected Vector3 _correctPosition = Vector3.zero;
        protected Quaternion _correctRotation = Quaternion.identity;
        protected Vector3 _correctVelocity = Vector3.zero;
        protected bool _syncVelocity = false;
        protected float SmoothingDelay => 5f;
        protected Transform _transform;
        protected Rigidbody _rigidbody;
        protected PhotonView _photonView;

        protected virtual void Awake()
        {
            photonView.observed = this;
            _transform = transform;
            _photonView = photonView;
            _correctPosition = _transform.position;
            _correctRotation = transform.rotation;
            _rigidbody = rigidbody;
            if (rigidbody != null)
            {
                _syncVelocity = true;
                _correctVelocity = _rigidbody.velocity;
            }
        }

        protected virtual void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.isWriting)
            {
                stream.SendNext(_transform.position);
                stream.SendNext(_transform.rotation);
                if (_syncVelocity)
                {
                    stream.SendNext(_rigidbody.velocity);
                }
            }
            else
            {
                _correctPosition = (Vector3)stream.ReceiveNext();
                _correctRotation = (Quaternion)stream.ReceiveNext();
                if (_syncVelocity)
                    _correctVelocity = (Vector3)stream.ReceiveNext();
            }
        }

        protected virtual void Update()
        {
            if (!Disabled && !_photonView.isMine)
            {
                _transform.position = Vector3.Lerp(_transform.position, _correctPosition, Time.deltaTime * SmoothingDelay);
                _transform.rotation = Quaternion.Lerp(_transform.rotation, _correctRotation, Time.deltaTime * SmoothingDelay);
                if (_syncVelocity)
                    _rigidbody.velocity = _correctVelocity;
            }
        }
    }
}