const HtmlWebpackPlugin = require('html-webpack-plugin');
const MiniCssExtractPlugin = require('mini-css-extract-plugin');

const htmlPlugin = new HtmlWebpackPlugin({
  inject: true,
  template: 'index.html',
  base: '/'
});

const cssPlugin = new MiniCssExtractPlugin();

module.exports = (env) => {
  const prod = !!env.production;
  const buildPath = `${__dirname}/${env.output ?? '../wwwroot'}`;

  htmlPlugin.minify = prod;

  return {
    mode: prod ? 'production' : 'development',
    entry: './src/index.tsx',
    output: {
      path: buildPath
    },
    module: {
      rules: [
        {
          test: /\.(ts|tsx)$/,
          exclude: /node_modules/,
          resolve: {
            extensions: ['.ts', '.tsx', '.js', '.json']
          },
          use: 'ts-loader'
        },
        {
          test: /\.scss$/,
          use: [prod ? MiniCssExtractPlugin.loader : 'style-loader', 'css-loader', 'sass-loader']
        }
      ]
    },
    devtool: prod ? undefined : 'source-map',
    plugins: prod ? [htmlPlugin, cssPlugin] : [htmlPlugin]
  };
};
