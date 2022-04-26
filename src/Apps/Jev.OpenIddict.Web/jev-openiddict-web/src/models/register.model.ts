export interface RegisterModel {
  email: string;
  password: string;
  confirmPassword: string;
}

export function GetDefaultRegisterModel(): RegisterModel {
  return {
    email: '',
    password: '',
    confirmPassword: ''
  };
}
