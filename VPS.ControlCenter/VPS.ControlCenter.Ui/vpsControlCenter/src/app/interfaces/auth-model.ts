
export interface Token {
  token: string;
  expiryTime: string;
}

export interface AccessToken extends Token {}

export interface RefreshToken extends Token {}

export interface AuthModel {
  accessToken: AccessToken;
  refreshToken: RefreshToken;
  isActive: boolean;
  userId: number;
  isSuccess: boolean | null;
  message: string | null;
  errorMessages: string[] | null;
}