import axios, { AxiosError, AxiosInstance, AxiosResponse } from 'axios';
import { AnyAction, Dispatch } from 'redux';
import { AlertActions } from '../actions/alert-state.actions';
import { ErrorResponseModel } from '../dto/error-response.dto';
import { RegisterDto } from '../dto/register.dto';
import { ApplicationStore } from '../store';

class JevOpenIddictService {
  private _axios: AxiosInstance;

  constructor(private _dispatch: Dispatch<AnyAction>) {
    this._axios = axios.create({
      baseURL: `${window.location.origin}/api`
    });
  }

  public register(registerModel: RegisterDto): Promise<string> {
    return new Promise<string>((resolve: (str: string) => void, reject: (error: any) => void) => {
      this._axios
        .post<string>('/account/register', registerModel)
        .then((response: AxiosResponse) => {
          resolve(response.data);
        })
        .catch((error: AxiosError<ErrorResponseModel>) => {
          this._dispatch(
            AlertActions.addToQueue({
              header: `Server error response: ${error.response?.data?.responseCode ?? '500'}`,
              message: error.response?.data?.message ?? 'Unkown error from server',
              type: 'danger'
            })
          );
          reject(error);
        });
    });
  }
}

export const OpenIddictService: JevOpenIddictService = new JevOpenIddictService(ApplicationStore.dispatch);
