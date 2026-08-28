import React, { createContext, useContext, useEffect, useState, useCallback } from 'react';
import { useDispatch } from 'react-redux';
import { useNavigate } from 'react-router-dom';
import { setUser, logOut } from '@/store/api/auth/authSlice';
import Loading from '@/components/Loading';

const KeycloakContext = createContext(null);

export const useKeycloak = () => {
  const context = useContext(KeycloakContext);
  if (!context) {
    throw new Error('useKeycloak must be used within a KeycloakProvider');
  }
  return context;
};

// ============================================================================
// AUTH BYPASS (demo mode)
// ----------------------------------------------------------------------------
// keycloak-js is intentionally NOT used here. It requires the Web Crypto API,
// which browsers only expose in a "secure context" (HTTPS or localhost) — so it
// breaks over plain HTTP on a LAN IP. Bypassing it lets the admin app run with
// no login on BOTH http://localhost:7002 and http://<host-ip>:7002. The Catalog
// API is correspondingly configured to allow anonymous requests (see
// Catalog.Api/DependencyInjection.cs). To restore real authentication, revert
// this file and that backend change from git history.
// ============================================================================

const MOCK_USER = {
  id: 'demo',
  username: 'demo',
  email: 'demo@progcoder.local',
  firstName: 'Demo',
  lastName: 'User',
  name: 'Demo User',
  roles: ['admin'],
};

export const KeycloakProvider = ({ children }) => {
  const [keycloakReady, setKeycloakReady] = useState(false);
  const [authenticated, setAuthenticated] = useState(false);
  const [loading, setLoading] = useState(true);
  const dispatch = useDispatch();
  const navigate = useNavigate();

  useEffect(() => {
    // No IdP round-trip: mark the session authenticated with a demo user.
    dispatch(setUser(MOCK_USER));
    localStorage.setItem('user', JSON.stringify(MOCK_USER));
    setKeycloakReady(true);
    setAuthenticated(true);
    setLoading(false);
  }, [dispatch]);

  const login = useCallback(() => {
    // Already "authenticated" in bypass mode.
    navigate('/', { replace: true });
  }, [navigate]);

  const logout = useCallback(() => {
    dispatch(logOut());
    localStorage.removeItem('user');
    navigate('/', { replace: true });
  }, [dispatch, navigate]);

  const value = {
    keycloakReady,
    authenticated,
    login,
    logout,
    getKeycloak: () => null,
    getUserInfo: () => {
      try {
        return JSON.parse(localStorage.getItem('user') || 'null');
      } catch {
        return null;
      }
    },
    updateToken: async () => true,
  };

  if (loading) {
    return <Loading />;
  }

  return (
    <KeycloakContext.Provider value={value}>
      {children}
    </KeycloakContext.Provider>
  );
};
